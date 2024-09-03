import { HttpHeaders, HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { AuthenticationResult, EventMessage, EventType, InteractionStatus, AccountInfo } from '@azure/msal-browser';
import { Observable, catchError, defer, filter, forkJoin, from, map, mergeMap, of, throwError } from 'rxjs';
import { GoogleClassworkDataService } from '../services/google-classwork-data.service';
import { EducationAssignment } from '../models/EducationAssignment';
import { EducationAssignmentResource } from '../models/EducationAssignmentResource';
import { TeamsMigrationService } from '../services/teams-migration.service';
import { environment } from '../../environments/environment';

interface GraphClassesResponse {
  value: { id: string, displayName: string }[];
}

@Component({
  selector: 'app-teamsclasses',
  templateUrl: './teamsclasses.component.html',
  styleUrl: './teamsclasses.component.scss'
})
export class TeamsclassesComponent implements OnInit {

  loginDisplay = false;
  activeAccount: AccountInfo | null = null;
  classes: { id: string, displayName: string }[] = [];
  selectedClass: { id: string, displayName: string } | null = null;
  selectedCourseworkList: any[] = [];
  selectedCourseMaterials: any[] = [];
  assignmentsCreated: any[] = [];
  headers: HttpHeaders = new HttpHeaders();
  loading = true;
  isSelected = false;
  createdAssignment: any;
  createdModules: any;
  disableFinish = true;

  constructor(private authService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
    private http: HttpClient,
    private route: ActivatedRoute,
    private googleClassworkDataService: GoogleClassworkDataService,
    private teamsMigrationService: TeamsMigrationService) { }

  async ngOnInit(): Promise<void> {
    try {
      this.loading = true;
      await this.authService.instance.initialize();
      this.activeAccount = this.authService.instance.getActiveAccount();
      if (!this.activeAccount && this.authService.instance.getAllAccounts().length > 0) {
        this.activeAccount = this.authService.instance.getAllAccounts()[0];
        this.authService.instance.setActiveAccount(this.activeAccount);
      }

      this.msalBroadcastService.msalSubject$
        .pipe(
          filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS)
        )
        .subscribe((result: EventMessage) => {
          console.log('Login success:', result);
          this.activeAccount = this.authService.instance.getAllAccounts()[0];
          this.authService.instance.setActiveAccount(this.activeAccount);
          this.setLoginDisplay();
          this.fetchClasses();
        });

      this.msalBroadcastService.inProgress$
        .pipe(
          filter((status: InteractionStatus) => status === InteractionStatus.None)
        )
        .subscribe(() => {
          this.setLoginDisplay();
        });

      this.setLoginDisplay();
      this.fetchClasses();
    } catch (error) {
      console.error('MSAL initialization error:', error);
    }
    this.loading = false;
  }

  setLoginDisplay() {
    this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
    if (this.loginDisplay) {
      this.fetchClasses();
    }
  }

  fetchClasses(): void {
    if (!this.activeAccount) {
      console.log('No active account found.');
      return;
    }

    this.authService.acquireTokenSilent({
      account: this.activeAccount,
      scopes: ['User.Read', 'EduRoster.ReadBasic']
    }).subscribe({
      next: (result: AuthenticationResult) => {
        this.headers = new HttpHeaders({
          'Authorization': `Bearer ${result.accessToken}`
        });

        sessionStorage.setItem('teamsAccessToken', result.accessToken);

        this.http.get<GraphClassesResponse>('https://graph.microsoft.com/v1.0/education/classes', { headers: this.headers })
          .subscribe(response => {
            console.log('Classes:', response);
            this.classes = response.value;
          });
      },
      error: (error) => {
        if (error.errorCode === 'interaction_in_progress') {
          console.log('Interaction in progress, cannot start a new one.');
          return;
        }
        console.error('Error acquiring token silently:', error);
        this.authService.acquireTokenRedirect({
          scopes: ['User.Read', 'EduRoster.ReadBasic']
        });
      }
    });
  }

  onClassSelected(event: any): void {
    const selectedClass = {
      id: event.target.value,
      displayName: this.classes.find(c => c.id === event.target.value)?.displayName || ''
    };
    console.log('Selected class:', selectedClass.id, selectedClass.displayName);
    this.selectedClass = selectedClass;
    this.disableFinish = false;
  }

  async onSelect(): Promise<void> {
    this.disableFinish = true;
    this.selectedCourseworkList = sessionStorage.getItem('selectedCourseWorkList') ? JSON.parse(sessionStorage.getItem('selectedCourseWorkList') || '') : [];
    this.selectedCourseMaterials = sessionStorage.getItem('selectedCourseMaterials') ? JSON.parse(sessionStorage.getItem('selectedCourseMaterials') || '') : [];
    console.log(`Selected coursework: ${this.selectedCourseworkList} Selected materials: ${this.selectedCourseMaterials}`);
    this.isSelected = true;
    this.loading = true;
    if (this.selectedClass) {
      this.createdAssignment = await this.mapCourseWorksToAssignments(this.selectedCourseworkList, this.selectedClass.id);
      this.createdModules = await this.mapCourseWorkMaterialsToModules(this.selectedCourseMaterials, this.selectedClass.id);
      
    }
    this.loading = false;
  }

  async mapCourseWorksToAssignments(courseWorks: any[], classId: string) {
    const assignmentsCreated: string[] = [];
    for(const courseWork of courseWorks){
      try{
        const assignment: EducationAssignment = {
          displayName: courseWork.title,
          instructions: { content: courseWork.description },
          dueDateTime: new Date(new Date().getTime() + 7 * 24 * 60 * 60 * 1000).toISOString()
        };
        const createdAssignment = await this.teamsMigrationService.createAssignment(classId, assignment).toPromise();
        assignmentsCreated.push(createdAssignment?.displayName ?? '');
        assignment.id = createdAssignment?.id;
        assignment.classId = createdAssignment?.classId;
        if (courseWork.materials && courseWork.materials.length > 0) {
          await this.mapMaterialToResources(courseWork.materials, assignment, true);
        }
        
      } catch (error) {
        console.error('Error mapping coursework to assignment:', error);
      }
    }
    alert('Assignments created successfully');
    return assignmentsCreated;
  }

  async mapCourseWorkMaterialsToModules(courseWorkMaterials: any[], classId: string): Promise<string[]> {
    console.log("* Importing coursework materials from Google Classroom into Microsoft Teams classwork...");
    const modulesCreated: string[] = [];

    for (const courseWork of courseWorkMaterials) {
      try {
        const createdModule = await this.teamsMigrationService.createModule(classId, courseWork.title, courseWork.description).toPromise();
        modulesCreated.push(createdModule.displayName);
        createdModule.id = createdModule?.id;
        createdModule.classId = classId;

        if (courseWork.materials && courseWork.materials.length > 0) {
          await this.mapMaterialToResources(courseWork.materials, createdModule, false);
        }
        
      } catch (error) {
        console.error('Error mapping coursework materials to modules:', error);
      }
    }
    alert('Modules created successfully');
    return modulesCreated;
  }

  async mapMaterialToResources(materials: any[], createdAssignment: EducationAssignment, isAssignment: boolean) {
    const classId = createdAssignment.classId || '';
    const assignmentName = createdAssignment.displayName || '';
    for (const material of materials) {
      if (material.driveFile) {
        const sourceFileMetadata = await this.googleClassworkDataService.getGoogleDriveFileMetadata(material.driveFile.driveFile.id);
        if (sourceFileMetadata.mimeType.includes('drawing')) {
          continue;
        }
        const targetFileTypeDetails = this.getFileDetails(sourceFileMetadata.mimeType);
        const isExport = targetFileTypeDetails.fileExtension && targetFileTypeDetails.fileExtension.trim() !== '';
        const fileAsByteArray = await this.googleClassworkDataService.getGoogleDoc(material.driveFile.driveFile.id, targetFileTypeDetails.fileMimeType, isExport, assignmentName);

        const fileName = `${material.driveFile.driveFile.title}${targetFileTypeDetails.fileExtension}`;
        if (fileName) {
          if (!createdAssignment.resourcesFolderUrl) {
            createdAssignment = await this.teamsMigrationService.setupResourcesFolder(createdAssignment.classId || '', createdAssignment.id || '', isAssignment, assignmentName);
          }
          const uploadUrl = `${createdAssignment.resourcesFolderUrl}:/${fileName}:/content`;
          const urlSegments = createdAssignment.resourcesFolderUrl.split('/');
          const driveId = urlSegments[urlSegments.length - 3];
          const itemId = urlSegments[urlSegments.length - 1];

          const driveItem = await this.teamsMigrationService.uploadFileToGraph(driveId, itemId, fileName, fileAsByteArray, assignmentName);
          const assignmentFileUrl = `${environment.teamsApiUrl}/drives/${driveId}/items/${driveItem.id}`;

          const assignmentResource: EducationAssignmentResource = {
            distributeForStudentWork: material.driveFile.shareMode === 'STUDENT_COPY',
            resource: this.getEducationResource(sourceFileMetadata.mimeType, assignmentFileUrl, fileName)
          };

          await this.teamsMigrationService.postResource(classId ?? '', createdAssignment.id ?? '', assignmentResource, isAssignment, assignmentName);
        }
      } else if (material.link) {
        const assignmentResource: EducationAssignmentResource = {
          distributeForStudentWork: false,
          resource: {
            link: material.link.url,
            displayName: material.link.title,
            '@odata.type': 'microsoft.graph.educationLinkResource',
          }
        };
        await this.teamsMigrationService.postResource(classId ?? '', createdAssignment.id ?? '', assignmentResource, isAssignment, assignmentName);
      } else if (material.youtubeVideo) {
        const assignmentResource: EducationAssignmentResource = {
          distributeForStudentWork: false,
          resource: {
            link: material.youtubeVideo.alternateLink,
            displayName: material.youtubeVideo.title,
            '@odata.type': 'microsoft.graph.educationLinkResource',
          }
        };
        await this.teamsMigrationService.postResource(classId || '', createdAssignment.id || '', assignmentResource,isAssignment, assignmentName);
      }
    }
  }

  getFileDetails(sourceMimeType: string): any {
    switch (sourceMimeType) {
      case 'application/vnd.google-apps.document':
        return { fileExtension: '.docx', fileMimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' };
      case 'application/vnd.google-apps.presentation':
        return { fileExtension: '.pptx', fileMimeType: 'application/vnd.openxmlformats-officedocument.presentationml.presentation' };
      case 'application/vnd.google-apps.spreadsheet':
        return { fileExtension: '.xlsx', fileMimeType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' };
      default:
        return { fileExtension: '', fileMimeType: sourceMimeType };
    }
  }

  getEducationResource(mimeType: string, fileUrl: string, fileName: string): any {
    if(mimeType.includes('image')){
        return {
          '@odata.type': mimeType,
          fileUrl: fileUrl,
          displayName: fileName
        }
      } else if(mimeType.includes('document')){
        return {
          '@odata.type': 'microsoft.graph.educationWordResource',
          fileUrl: fileUrl,
          displayName: fileName
        };
      } else if(mimeType.includes('pdf')){
        return {
          '@odata.type': 'microsoft.graph.educationFileResource',
          fileUrl: fileUrl,
          displayName: fileName
        }
      } else if(mimeType.includes('spreadsheet')){
        return {
          '@odata.type': 'microsoft.graph.educationExcelResource',
          fileUrl: fileUrl,
          displayName: fileName
        }
      } else if(mimeType.includes('presentation')){
        return {
          '@odata.type': 'microsoft.graph.educationPowerPointResource',
          fileUrl: fileUrl,
          displayName: fileName
        }
      }
      else{
        return {
          '@odata.type': 'microsoft.graph.educationLinkResource',
          link: fileUrl,
          displayName: fileName
        };
    }
  }
}

