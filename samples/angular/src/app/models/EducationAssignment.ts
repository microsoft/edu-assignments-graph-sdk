export interface EducationAssignment {
    resourcesFolderUrl?: any;
    displayName: string;
    instructions: {
      content: string;
    };
    dueDateTime: string;
    assignTo?: {
      studentIds?: string[];
      studentGroupIds?: string[];
    };
    grading?: {
      gradingScaleType?: string;
      maxPoints?: number;
    };
    status?: string;
    classId?: string;
    id?: string;
  }