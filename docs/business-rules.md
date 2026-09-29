# Dawaya business rules

## 1. Scope
BR-01: The app explains registered medicines: what they're for, how to take them, side effects, and warnings. It does not diagnose, prescribe, or recommend medicines.
BR-02: If a user's question is outside medicine explanation (e.g. "what should I take for my headache?"), the app declines politely and recommends a pharmacist or doctor.
BR-03: The app covers medicines registered in Egypt first. Unregistered or imported items show a "limited information" notice.

## 2. Users and accounts
BR-04: Guests can scan and view medicine information without an account.
BR-05: Registered users can save medicines, set reminders, and view history.
BR-06: A registered user can create family profiles (e.g. "Mom", "Dad") and manage medicines for each, since caregivers are a main target group.
BR-07: Each profile can store optional details that affect warnings only: age group, pregnancy/breastfeeding, and known allergies. These never change dosage shown.

## 3. Scanning and identification
BR-08: Users can identify a medicine by photographing the box, scanning the barcode, or typing the name.
BR-09: The AI reads the image, but the identified name must be matched against the medicine database before any information is shown. The AI never describes a medicine that isn't in the database.
BR-10: If match confidence is below a set threshold (e.g. 85%), the app shows the top 3 candidates and asks the user to confirm.
BR-11: The user must confirm the strength and form (e.g. 500mg tablet vs. syrup), since dosage differs between them.
BR-12: If nothing matches, the app says so clearly and never guesses.

## 4. Information source
BR-13: All medical content comes from the official leaflet or the medicine database. The AI's role is to simplify and translate that content, not add new facts.
BR-14: Every information screen shows its source and the date the data was last updated.
BR-15: Admins must review AI-simplified text for a medicine before it's marked "verified." Unverified content is shown with a visible label.

## 5. Displaying information
BR-16: Each medicine page shows, in this order: what it's for, how to take it, common side effects, serious side effects, who shouldn't take it, and storage.
BR-17: Language defaults to simple Egyptian Arabic, with English available. Medical terms get a plain explanation next to them.
BR-18: Serious side effects and warnings are always visible, never hidden behind "read more."

## 6. Dosage rules (the most sensitive part)
BR-19: The app shows the standard dosage from the leaflet only, labeled as "the usual dose according to the leaflet."
BR-20: Every dosage section includes: "If your doctor prescribed a different dose, follow your doctor."
BR-21: The app never calculates personalized doses (e.g. by child's weight) and never suggests increasing, decreasing, or stopping a dose.
BR-22: For children's doses, the app shows the leaflet's age ranges and strongly recommends confirming with a pharmacist.

## 7. Safety and warnings
BR-23: Every medicine page shows a fixed disclaimer: this is general information and doesn't replace a doctor or pharmacist.
BR-24: If the active profile is pregnant, breastfeeding, a child, or elderly, relevant leaflet warnings are highlighted at the top.
BR-25: If the profile has a recorded allergy that matches an ingredient, a red warning appears before anything else.
BR-26: If a user's question suggests an overdose, poisoning, or serious reaction, the app stops answering normally and shows emergency guidance: call 123 (ambulance) and contact a poison control center.
BR-27: For controlled or misuse-prone medicines, the app shows standard leaflet information only and doesn't answer questions about exceeding doses or combining for effect.

## 8. Interactions
BR-28: When a profile has 2 or more saved medicines, the app checks known interactions from the database.
BR-29: Interactions are shown by severity (major, moderate, minor), with major ones advising the user to consult a pharmacist before continuing.
BR-30: If interaction data is unavailable for a combination, the app says "no data available," never "safe together."

## 9. Reminders
BR-31: Users can set reminders per medicine and profile, using times they enter themselves.
BR-32: Reminders can be marked taken or skipped, and a caregiver can see this history for family profiles.

## 10. Accessibility (key for elderly users)
BR-33: Large text mode and high contrast are available from the first screen.
BR-34: Every medicine page has a "read aloud" option in Arabic.
BR-35: Main flows (scan, view, remind) must work in 3 taps or fewer.

## 11. Privacy and data
BR-36: Health data (profiles, medicines, allergies) is sensitive and is encrypted at rest and in transit.
BR-37: Scanned images are deleted after identification unless the user chooses to save them.
BR-38: Users can export or permanently delete all their data at any time.
BR-39: The app complies with Egypt's Personal Data Protection Law (Law No. 151 of 2020). Verify the current requirements for health data before launch.

## 12. Admin
BR-40: Admins can add, edit, and verify medicines, and each change is logged with who made it and when.
BR-41: Users can report wrong information, and reports go to an admin review queue.
BR-42: Medicines withdrawn or recalled by the Egyptian Drug Authority are flagged with a visible warning.

## 13. Prescription scanning
BR-43: Users can photograph a doctor's prescription (handwritten or printed) to extract the medicines and instructions.
BR-44: The AI extracts each item as: medicine name, strength, dose, frequency, timing (before/after meals), and duration. Each extracted medicine must be matched against the medicine database (same as BR-09).
BR-45: Every extracted item has a confidence score. Items above the threshold are shown for confirmation; items below it are marked "unclear."
BR-46: The user must review and confirm every item before it's saved, with the original prescription image shown side by side with the extracted text.
BR-47: The app recommends that the user shows the extracted result to their pharmacist before starting a new prescription, since handwriting misreads can be dangerous (e.g. similar-looking drug names).
BR-48: Once confirmed, the prescription is saved to the patient's profile with the doctor's name and date, if readable.

## 14. When the AI can't read it
BR-49: If a medicine name, strength, or dose is unclear or below the confidence threshold, the app never guesses. It shows: "We couldn't read this part clearly. Please ask your pharmacist or doctor to confirm it."
BR-50: Unclear items can't be added to the schedule or reminders until the user enters the confirmed details manually after checking with a pharmacist or doctor.
BR-51: If the whole prescription is unreadable, the app suggests retaking the photo (better lighting, flat surface) once, then directs the user to the pharmacist or doctor.

## 15. Dose schedule and explanation
BR-52: Confirmed medicines are organized into a daily schedule (morning, afternoon, evening, bedtime), grouped by time rather than by medicine.
BR-53: Each dose is explained in simple Arabic, e.g. "1 tablet after breakfast, with water."
BR-54: The doctor's prescribed dose is what the schedule uses. If it differs from the leaflet's usual dose, the app shows both, labeled clearly, and says to follow the doctor. It never flags the doctor's dose as "wrong."
BR-55: If two scheduled medicines have a major interaction (BR-28), the app shows a warning and advises confirming with the pharmacist, but never changes the schedule by itself.

This updates BR-19 to BR-21: the app now shows the doctor's prescribed dose from a confirmed prescription, but still never calculates or adjusts doses itself.

## 16. Tracking doses
BR-56: Users mark each scheduled dose as taken, skipped, or delayed, with the time recorded.
BR-57: A missed dose shows the leaflet's missed-dose guidance if available; otherwise, "ask your pharmacist." The app never suggests taking a double dose.
BR-58: Users (and caregivers for family profiles) can view adherence history per medicine, e.g. "taken 26 of 28 doses."

## 17. Treatment duration
BR-59: Each medicine has a duration: a fixed number of days, an end date, or "ongoing" (chronic medicines).
BR-60: Duration is taken from the prescription if readable, otherwise entered by the user.
BR-61: When a course ends, reminders stop automatically, and the app shows a note if the medicine is one that shouldn't be stopped suddenly according to its leaflet.

## 18. Refill alerts
BR-62: Users enter the quantity per pack when adding a medicine (e.g. 20 tablets).
BR-63: Remaining quantity decreases automatically each time a dose is marked taken.
BR-64: The app estimates the run-out date based on remaining quantity and the schedule.
BR-65: If the medicine will run out before the course ends (or it's ongoing), the user gets a refill notification a set number of days before running out (default 3, adjustable).
BR-66: The notification offers options: order from a nearby pharmacy through the app, call the pharmacy, or dismiss.
BR-67: No refill notification is sent if the course ends before the medicine runs out.

## 19. Nearby pharmacies
BR-68: With the user's permission, the app uses their location to show nearby pharmacies on a map, with distance, working hours, phone number, and whether they deliver.
BR-69: If location permission is denied, users can search by area name instead.
BR-70: Partner pharmacies can receive orders through the app. Non-partner pharmacies show "call" and "directions" only.
BR-71: Prescription-only medicines can only be ordered with an attached valid prescription, which the pharmacist verifies before dispensing. The app doesn't bypass pharmacist approval.
BR-72: The app never shares a user's health data with a pharmacy except the specific order and prescription the user chooses to send.

## 20. Medical records and QR sharing
BR-73: Users can upload medical records to a profile: lab tests, X-rays, scans, reports, and past prescriptions, each tagged with type and date.
BR-74: Users can generate a QR code to share records with a doctor. The QR code contains a secure link, never the medical data itself.
BR-75: When generating the QR code, the user chooses what to share (all records or selected ones) and how long the link stays valid (e.g. 1 hour, 24 hours).
BR-76: Scanning the QR code opens a read-only view of the shared records. It expires automatically and can be revoked by the user at any time.
BR-77: Every access to shared records is logged, and the user can see when their records were opened.
BR-78: The app doesn't interpret lab results or X-rays with AI in the MVP. It stores and shares them only. Interpreting results is a separate, much higher-risk feature that would need medical review.

## 21. Manual medicine entry
BR-79: Users can add a medicine to a profile manually by typing it, as an alternative to scanning a box or prescription.
BR-80: While typing the name, the app suggests matching medicines from the database (autocomplete in Arabic and English) to reduce spelling errors.
BR-81: After choosing a medicine, the user selects its strength and form from the available options (e.g. 500mg tablet, 250mg/5ml syrup).
BR-82: The user enters the doctor's instructions using structured fields rather than free text: dose amount and unit, times per day, specific times, before/after meals, duration (BR-59), and pack quantity (BR-62).
BR-83: An optional notes field lets the user add any extra instruction from the doctor (e.g. "dissolve in water").
BR-84: Before saving, the app shows a plain-Arabic summary for confirmation, e.g. "Panadol 500mg: 1 tablet, 3 times a day after meals, for 5 days."
BR-85: If the entered dose is above the leaflet's maximum daily dose, the app asks the user to double-check what they typed and confirm with their doctor or pharmacist. This is a typo check, not a judgment on the doctor's dose (consistent with BR-54), and the user can still save after confirming.
BR-86: Once saved, a manually added medicine works the same as a scanned one: it appears in the daily schedule, gets reminders, dose tracking, interaction checks, and refill alerts.
BR-87: If the medicine isn't found in the database, the user can still add it as a custom medicine for reminders only. It's clearly labeled "not in our database," with no leaflet information, and interaction checks show "no data available" (BR-30).
BR-88: Each saved medicine records how it was added (box scan, prescription scan, or manual entry), and users can edit or delete it at any time.