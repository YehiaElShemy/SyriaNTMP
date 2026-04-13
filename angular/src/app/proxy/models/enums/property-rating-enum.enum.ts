import { mapEnumToOptions } from '@abp/ng.core';

export enum PropertyRatingEnum {
  OneStar = 1,
  TwoStar = 2,
  ThreeStar = 3,
  FourStar = 4,
  FiveStar = 5,
}

export const propertyRatingEnumOptions = mapEnumToOptions(PropertyRatingEnum);
