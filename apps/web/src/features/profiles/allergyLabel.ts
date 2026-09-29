import type { Schemas } from 'api-client';

type Allergy = Schemas['AllergyResponse'];

// Linked allergies show the ingredient name in the current language; typed ones show the text as entered.
export function allergyLabel(allergy: Allergy, language: string): string {
  const linked = language === 'ar' ? (allergy.ingredientNameAr ?? allergy.ingredientNameEn) : allergy.ingredientNameEn;
  return linked ?? allergy.allergyText ?? '';
}
