import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { EducationAssignmentResource } from '../models/EducationAssignmentResource';
import { Observable, catchError, map, throwError } from 'rxjs';
import { EducationAssignment } from '../models/EducationAssignment';
import { Configuration } from 'msal';
import { PublicClientApplication } from '@azure/msal-browser';
import { MsalService } from '@azure/msal-angular';
import { AuthenticationProvider, Client } from '@microsoft/microsoft-graph-client';

class CustomAuthProvider implements AuthenticationProvider {
  async getAccessToken(): Promise<string> {
    const accessToken = sessionStorage.getItem('teamsAccessToken');
    if (!accessToken) {
      throw new Error('No access token found in sessionStorage.');
    }
    return accessToken;
  }
}
@Injectable({
  providedIn: 'root'
})
export class TeamsMigrationService {
  private client: Client;

  constructor(private http: HttpClient,
          private msalService: MsalService
  ) {
    const authProvider = new CustomAuthProvider();
    this.client = Client.initWithMiddleware({ authProvider });
   }

  private headers = new HttpHeaders();

  initializeHeader(){
    this.headers = new HttpHeaders({
      'Authorization': `Bearer ${sessionStorage.getItem('teamsAccessToken')}`
    });
  }

  async setupResourcesFolder(classId: string, id: string, isAssignment: boolean, assignment: string): Promise<any> {
    this.initializeHeader();
    if (!classId || !id) {
      console.log('Invalid classId or assignmentId/ moduleId');
      alert('Invalid classId or assignmentId/moduleId');
      return;
    }
  
    try {
      let url;
      if(isAssignment){
        url= `${environment.teamsApiUrl}/education/classes/${classId}/assignments/${id}/setupResourcesFolder`;
      }else{
        url = `${environment.teamsApiUrl}/education/classes/${classId}/modules/${id}/setUpResourcesFolder`;
      }
       const response = await this.http.post(url, null, { headers: this.headers }).toPromise();
      return response;
    } catch (error) {
      console.error('Error setting up resources folder:', error);
      alert('Error setting up resources folder for ' + assignment);
      throw new Error('Failed to set up resources folder');
    }
  }

  async uploadFileToGraph(driveId: string, itemId: string, fileName: string, fileAsByteArray: Uint8Array, assignment: string): Promise<any> {
    this.initializeHeader();
    const url = `${environment.teamsApiUrl}/drives/${driveId}/items/${itemId}:/${fileName}:/content`;
    try {
      const response = await fetch(url, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/octet-stream',
          'Authorization': `Bearer ${sessionStorage.getItem('teamsAccessToken')}`,
        },
        body: fileAsByteArray,
      });
  
      if (!response.ok) {
        throw new Error(`Failed to upload file to Graph API. Status: ${response.status}`);
      }
      return response.json();
    } catch (error) {
      console.error('Error uploading file to Graph API:', error);
      alert('Error uploading file to Teams for ' + assignment);
      throw new Error('Failed to upload file to Graph API');
    }
  }
  

  async postResource(classId: string, assignmentId: string, resource: EducationAssignmentResource, isAssignment: boolean, assignment: string): Promise<any> {
    if (!classId || !assignmentId || !resource) {
      console.log('Invalid parameters for posting resource.');
      alert('Invalid parameters for posting resource for ' + assignment);
      return;
    }
  
    try {
      let response;
      if(isAssignment){
        response = await this.client
        .api(`/education/classes/${classId}/assignments/${assignmentId}/resources`)
        .post(resource);
      } else {
        response = await this.client
        .api(`/education/classes/${classId}/modules/${assignmentId}/resources`)
        .post(resource);
      }
      

      return response;
    } catch (error) {
      console.error('Error posting resource:', error);
      alert('Error adding resources for ' + assignment + ' to Teams');
      throw new Error('Failed to post resource');
    }
  }

  createModule(classId: string, displayName: string, description: string): Observable<any> {
    const url = `${environment.teamsApiUrl}/education/classes/${classId}/modules`;
    const accessToken = sessionStorage.getItem('teamsAccessToken');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    });
    const body = {
      displayName: displayName,
      description: description
    };
  
    return this.http.post<any>(url, body, { headers: headers }).pipe(
      map(response => {
        return { ...response, id: response.id };
      }),
      catchError(error => {
        console.error('Error creating module:', error);
        alert('Error creating module: '+ displayName);
        return throwError('Error creating module');
      })
    );
  }

  createAssignment(classId: string, assignment: EducationAssignment): Observable<EducationAssignment> {
    this.initializeHeader();
    return this.http.post<EducationAssignment>(`${environment.teamsApiUrl}/education/classes/${classId}/assignments`,
      assignment,
      { headers: this.headers }
    ).pipe(
      map(response => {
        return { ...response, classId: classId, id: response.id };
      }),
      catchError(error => {
        console.error('Error creating Assignment:', error);
        alert('Error creating Assignment: ' + assignment?.displayName);
        return throwError('Error creating Assignment');
      })
    );
  }
}
