import { EducationResource } from "./EducationResource";

export interface EducationAssignmentResource {
    distributeForStudentWork?: boolean;
    resource?: EducationResource; // Assuming EducationResource is another model
  }