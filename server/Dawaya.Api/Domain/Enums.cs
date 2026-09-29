namespace Dawaya.Api.Domain;

public enum AgeGroup { Infant, Child, Adult, Elderly }

public enum RegistrationStatus { Registered, LimitedInfo }

public enum ContentLanguage { Ar, En }

// Order follows BR-16, plus MissedDose (BR-57)
public enum ContentSection
{
    Uses = 1,
    HowToTake,
    CommonSideEffects,
    SeriousSideEffects,
    WhoShouldNotTake,
    Storage,
    MissedDose
}

public enum ContentStatus { Unverified, Verified }              // BR-15
public enum SourceType { Leaflet, MedicineDatabase, EdaRegistry }
public enum WarningTargetGroup { General, Pregnancy, Breastfeeding, Child, Elderly } // BR-24
public enum WarningSeverity { Info, Caution, Serious }
public enum InteractionSeverity { Minor = 1, Moderate = 2, Major = 3 }               // BR-29
