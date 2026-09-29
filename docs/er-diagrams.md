# Dawaya: ER diagrams

Five focused diagrams covering the whole schema. All tables also carry `CreatedAt` / `UpdatedAt` (omitted here for readability). Legend: `PK` primary key, `FK` foreign key, `UK` unique.

**Decisions built in:**
1. `ProfileMedications.Status` is `Saved` (no regimen) or `Active` once a regimen exists.
2. Custom medicines get refill alerts, so the quantity fields live on `ProfileMedications`, not only on the product.
3. The max-daily-dose check is skipped for Child and Infant profiles.
4. `QuantityUnit` and `DoseUnit` must match, enforced through `MedicineProducts.UnitOfCount`.
5. A dose becomes `Missed` after a grace window: 2 hours, or halfway to the next dose if shorter (a system setting, not a column).
6. Guests may ask the AI, rate-limited per device/IP (`AiQuestionLogs.ClientKeyHash`).

---

## 1. Identity, profiles and allergies

```mermaid
erDiagram
    AspNetUsers ||--o{ Profiles : owns
    AspNetUsers ||--o{ PushSubscriptions : registers
    AspNetUsers ||--o{ Notifications : receives
    Profiles ||--o{ ProfileAllergies : has
    Profiles ||--o{ Notifications : "about"
    ActiveIngredients |o--o{ ProfileAllergies : "mapped to"

    AspNetUsers {
        string Id PK
        string Email UK
        string PreferredLanguage "ar or en"
        bool LargeText
        bool HighContrast
        string Role "User, Admin, Pharmacist"
    }
    Profiles {
        int Id PK
        string OwnerUserId FK
        string DisplayName
        string Relationship "Mom, Dad..."
        bool IsSelf
        string AgeGroup "Infant, Child, Adult, Elderly"
        bool IsPregnant
        bool IsBreastfeeding
    }
    ProfileAllergies {
        int Id PK
        int ProfileId FK
        int ActiveIngredientId FK "nullable"
        string AllergyText "for unmapped allergies"
    }
    ActiveIngredients {
        int Id PK
        string NameEn
        string NameAr
    }
    PushSubscriptions {
        int Id PK
        string UserId FK
        string Endpoint
        string P256dh
        string Auth
    }
    Notifications {
        int Id PK
        string UserId FK
        int ProfileId FK
        string Type "DoseReminder, Refill, CourseEnded, Recall, OrderUpdate"
        int RelatedEntityId
        datetime ScheduledFor
        datetime SentAt
        datetime ReadAt
        datetime DismissedAt
    }
```

---

## 2. Medicine catalog, interactions, content and admin

```mermaid
erDiagram
    Medicines ||--|{ MedicineProducts : "has strengths/forms"
    Medicines ||--o{ MedicineContents : "leaflet sections"
    Medicines ||--o{ MedicineWarnings : has
    Medicines ||--o{ MedicineRecalls : "may have"
    MedicineProducts ||--o{ MedicineRecalls : "specific product"
    MedicineProducts ||--|{ ProductIngredients : contains
    MedicineProducts ||--o{ ProductDoseRules : "per age group"
    ActiveIngredients ||--o{ ProductIngredients : "used in"
    ActiveIngredients ||--o{ Interactions : "ingredient A"
    ActiveIngredients ||--o{ Interactions : "ingredient B"
    Medicines ||--o{ ContentReports : "reported on"
    MedicineContents |o--o{ ContentReports : "specific section"
    AspNetUsers ||--o{ ContentReports : files
    AspNetUsers ||--o{ MedicineContents : verifies

    Medicines {
        int Id PK
        string BrandNameEn
        string BrandNameAr
        string Manufacturer
        string RegistrationStatus "Registered or Limited"
        bool IsControlled
        bool IsPrescriptionOnly
        bool NoSuddenStop
        bool IsRecalled
    }
    MedicineProducts {
        int Id PK
        int MedicineId FK
        string Form "tablet, syrup..."
        string StrengthText
        decimal StrengthValue
        string StrengthUnit
        string Barcode UK "nullable"
        string UnitOfCount "tablet, ml... drives dose and pack units"
    }
    ActiveIngredients {
        int Id PK
        string NameEn
        string NameAr
    }
    ProductIngredients {
        int Id PK
        int ProductId FK
        int ActiveIngredientId FK
        decimal Amount
        string Unit
    }
    ProductDoseRules {
        int Id PK
        int ProductId FK
        string AgeGroup
        string AgeRangeText
        string UsualDoseTextAr
        string UsualDoseTextEn
        decimal MaxDailyAmount "null for weight-based"
        string MaxDailyUnit
    }
    MedicineContents {
        int Id PK
        int MedicineId FK
        string Language
        string Section "Uses, HowToTake, ... MissedDose"
        string Body
        string SourceType
        string SourceRef
        datetime SourceUpdatedAt
        bool IsAiSimplified
        string Status "Unverified or Verified"
        string VerifiedById FK
        datetime VerifiedAt
        int Version
    }
    MedicineWarnings {
        int Id PK
        int MedicineId FK
        string TargetGroup "Pregnancy, Breastfeeding, Child, Elderly, General"
        string Severity
        string TextAr
        string TextEn
        string Status
    }
    Interactions {
        int Id PK
        int IngredientAId FK "A less than B"
        int IngredientBId FK
        string Severity "Major, Moderate, Minor"
        string DescriptionAr
        string DescriptionEn
        string Source
        datetime UpdatedAt
    }
    MedicineRecalls {
        int Id PK
        int MedicineId FK
        int ProductId FK "nullable"
        string Batches
        datetime AnnouncedAt
        string SourceRef
        string Note
    }
    MedicalTerms {
        int Id PK
        string TermAr
        string TermEn
        string PlainExplanationAr
    }
    ContentReports {
        int Id PK
        string ReporterUserId FK
        int MedicineId FK
        int ContentId FK "nullable"
        string Reason
        string Details
        string Status
        string ResolvedById FK
        datetime ResolvedAt
        string ResolutionNote
    }
    AuditLogs {
        int Id PK
        string UserId
        string EntityType
        string EntityId
        string Action
        json OldValues
        json NewValues
        datetime At
    }
```

---

## 3. Scanning, AI questions and prescriptions

```mermaid
erDiagram
    AspNetUsers |o--o{ ScanSessions : "starts (guest = null)"
    Profiles |o--o{ ScanSessions : "for profile"
    ScanSessions ||--o{ ScanCandidates : "top 3"
    Medicines ||--o{ ScanCandidates : "candidate"
    MedicineProducts |o--o{ ScanSessions : "confirmed product"
    AspNetUsers |o--o{ AiQuestionLogs : asks
    Medicines |o--o{ AiQuestionLogs : "about"
    Profiles ||--o{ Prescriptions : has
    Prescriptions ||--|{ PrescriptionItems : contains
    MedicineProducts |o--o{ PrescriptionItems : "matched to"

    ScanSessions {
        int Id PK
        string UserId FK "nullable for guests"
        int ProfileId FK "nullable"
        string ScanType "Box, Barcode, Text"
        string ImageBlobKey
        bool ImageRetained
        datetime ImageDeletedAt
        int SelectedProductId FK
        string Status
    }
    ScanCandidates {
        int Id PK
        int ScanSessionId FK
        int MedicineId FK
        decimal Confidence
        int Rank
    }
    AiQuestionLogs {
        int Id PK
        string UserId FK "nullable for guests"
        string ClientKeyHash "device/IP hash for rate limiting"
        int MedicineId FK "nullable"
        string Classification "InScope, OutOfScope, Emergency, MisuseBlocked"
        datetime CreatedAt
    }
    Prescriptions {
        int Id PK
        int ProfileId FK
        string ImageBlobKey
        string DoctorName
        date PrescriptionDate
        string Status "Processing, PendingReview, Confirmed, Unreadable"
        int RetakeCount "max 1"
        datetime ConfirmedAt
    }
    PrescriptionItems {
        int Id PK
        int PrescriptionId FK
        string RawText
        string ExtractedName
        string ExtractedStrength
        string ExtractedDose
        string ExtractedFrequency
        string ExtractedTiming
        string ExtractedDuration
        decimal Confidence
        int MatchedProductId FK "nullable"
        json BoundingBox
        string Status "Pending, Unclear, Confirmed, ManuallyResolved, Rejected"
        datetime ConfirmedAt
    }
```

---

## 4. Profile medications, regimens, schedule and dose tracking

```mermaid
erDiagram
    Profiles ||--o{ ProfileMedications : takes
    MedicineProducts |o--o{ ProfileMedications : "database medicine"
    PrescriptionItems |o--o| ProfileMedications : "created from"
    ProfileMedications ||--o{ Regimens : "versioned instructions"
    Regimens ||--|{ RegimenTimes : "dose times"
    RegimenTimes ||--o{ DoseLogs : "logged as"
    AspNetUsers ||--o{ DoseLogs : "recorded by"

    ProfileMedications {
        int Id PK
        int ProfileId FK
        int ProductId FK "null when custom"
        string CustomName "custom only"
        bool IsCustom
        string AddedVia "BoxScan, Barcode, PrescriptionScan, Manual"
        int PrescriptionItemId FK "nullable"
        string Status "Saved, Active, Completed, Stopped"
        decimal PackQuantity
        decimal RemainingQuantity
        string QuantityUnit "must match dose unit"
        int RefillLeadDays "default 3"
        bool RefillAlertsOn
        bool IsDeleted
    }
    Regimens {
        int Id PK
        int ProfileMedicationId FK
        decimal DoseAmount
        string DoseUnit "must match QuantityUnit"
        int TimesPerDay
        string MealTiming "Before, After, With, Any"
        string DurationType "FixedDays, EndDate, Ongoing"
        int DurationDays
        date StartDate
        date EndDate
        string Notes
        string DoseSource "Prescription, Manual, Leaflet"
        bool ExceedsMaxConfirmed
        datetime EffectiveFrom
        datetime EffectiveTo "null = current"
    }
    RegimenTimes {
        int Id PK
        int RegimenId FK
        time TimeOfDay
        string Slot "Morning, Afternoon, Evening, Bedtime"
        decimal DoseAmount "per-time override"
        bool ReminderOn
    }
    DoseLogs {
        int Id PK
        int RegimenTimeId FK "UK with ScheduledFor"
        datetime ScheduledFor "UK with RegimenTimeId"
        string Status "Taken, Skipped, Delayed, Missed"
        datetime ActionAt
        string RecordedByUserId FK
    }
```

---

## 5. Pharmacies, orders, medical records and QR sharing

```mermaid
erDiagram
    Pharmacies ||--o{ PharmacyHours : "opens"
    Pharmacies ||--o{ PharmacyStaff : employs
    AspNetUsers ||--o{ PharmacyStaff : "pharmacist account"
    AspNetUsers ||--o{ PharmacyOrders : places
    Profiles ||--o{ PharmacyOrders : "for profile"
    Pharmacies ||--o{ PharmacyOrders : receives
    Prescriptions |o--o{ PharmacyOrders : "attached"
    PharmacyOrders ||--|{ PharmacyOrderItems : contains
    MedicineProducts ||--o{ PharmacyOrderItems : "ordered"
    Profiles ||--o{ MedicalRecords : stores
    Profiles ||--o{ ShareLinks : shares
    ShareLinks ||--o{ ShareLinkRecords : "selected records"
    MedicalRecords ||--o{ ShareLinkRecords : "included in"
    ShareLinks ||--o{ ShareAccessLogs : "opened"

    Pharmacies {
        int Id PK
        string NameAr
        string NameEn
        string Area
        string Address
        geography Location "spatial index"
        string Phone
        bool HasDelivery
        bool IsPartner
        bool IsActive
    }
    PharmacyHours {
        int Id PK
        int PharmacyId FK
        int DayOfWeek
        time Opens
        time Closes
    }
    PharmacyStaff {
        int Id PK
        string UserId FK
        int PharmacyId FK
    }
    PharmacyOrders {
        int Id PK
        string UserId FK
        int ProfileId FK
        int PharmacyId FK
        int PrescriptionId FK "required if any Rx-only item"
        string Status "Submitted, UnderReview, Approved, Rejected, Ready, Delivered"
        string DeliveryType
        string DeliveryAddress
        string ReviewedById FK
        datetime ReviewedAt
        string RejectionReason
    }
    PharmacyOrderItems {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Quantity
    }
    MedicalRecords {
        int Id PK
        int ProfileId FK
        string RecordType "LabTest, XRay, Scan, Report, Prescription"
        string Title
        date RecordDate
        string BlobKey
        string ContentType
        long SizeBytes
        bool IsDeleted
    }
    ShareLinks {
        int Id PK
        int ProfileId FK
        string CreatedByUserId FK
        string TokenHash "SHA-256, raw token only in QR"
        string Scope "All or Selected"
        datetime ExpiresAt
        datetime RevokedAt
    }
    ShareLinkRecords {
        int Id PK
        int ShareLinkId FK
        int MedicalRecordId FK
    }
    ShareAccessLogs {
        int Id PK
        int ShareLinkId FK
        datetime AccessedAt
        string IpHash
        string UserAgent
    }
```
