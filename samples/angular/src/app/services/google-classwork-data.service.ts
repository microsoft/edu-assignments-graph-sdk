import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GoogleClassworkDataService {
  private headers = new HttpHeaders();

  constructor(private http: HttpClient) { }
  private accessToken = sessionStorage.getItem('googleAccessToken');
  initializeHeader(){
     this.headers = new HttpHeaders({
      'Authorization': `Bearer ${this.accessToken}`
    });
  }

  fetchCourses(): Observable<any> {
   this.initializeHeader();
    const url = environment.baseGoogleApiUrl + '/courses';
    return this.http.get(url, { headers: this.headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error fetching courses:', error);
        let errorMessage = 'Error occurred while fetching courses. Please try again later.';
        if (error.error instanceof ErrorEvent) {
          // Client-side error
          console.log(`An error occurred: ${error.error.message}`);
          errorMessage = `An error occurred: ${error.error.message}`;
        } else {
          // Server-side error
          console.log(`Server returned error ${error.status}: ${error.message}`)
          errorMessage = `401 Unauthorized. Please login again.`;
        }
        alert(errorMessage);
        return throwError(errorMessage);
      })
    );
  }

  fetchCourseWorkMaterials(courseId: string): Observable<any> {
    this.initializeHeader();
    const url = `${environment.baseGoogleApiUrl}/courses/${courseId.trim()}/courseWorkMaterials`;
    return this.http.get(url, { headers: this.headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error fetching course work materials for courseId ${courseId}:`, error);
        let errorMessage = 'Error occurred while fetching course work materials. Please try again later.';
        if (error.error instanceof ErrorEvent) {
          // Client-side error
          console.log(`An error occurred: ${error.error.message}`);
          errorMessage = `An error occurred. Please try after sometime.`;
        } else {
          // Server-side error
          console.log(`Server returned error ${error.status}: ${error.message}`)
          errorMessage = `401 Unauthorized. Please login again.`;
        }
        alert(errorMessage);
        return throwError(errorMessage);
      })
    );
  }

  fetchCourseWorkList(courseId: string): Observable<any> {
    this.initializeHeader();
    const url = `${environment.baseGoogleApiUrl}/courses/${courseId.trim()}/courseWork?courseWorkStates=DRAFT&courseWorkStates=PUBLISHED`;
    return this.http.get(url, { headers: this.headers }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error fetching course work list for courseId ${courseId}:`, error);
        let errorMessage = 'Error occurred while fetching course work list. Please try again later.';
        if (error.error instanceof ErrorEvent) {
          // Client-side error
          console.log(`An error occurred: ${error.error.message}`);
          errorMessage = `An error occurred: ${error.error.message}`;
        } else {
          // Server-side error
          console.log(`Server returned error ${error.status}: ${error.message}`)
          errorMessage = `401 Unauthorized. Please login again.`;
        }
        alert(errorMessage);
        return throwError(errorMessage);
      })
    );
  }
  
  
  async getGoogleDoc(fileId: string, mimeType: string, isExport: boolean, assignment: string): Promise<Uint8Array> {
    const query = isExport? `/export?mimeType=${mimeType}` : `?alt=media`;
    const url = `${environment.googleApiUrl}/drive/v3/files/${fileId}${query}`;
    
    try {
      const response = await fetch(url, {
        headers: {
          'Authorization': `Bearer ${this.accessToken}`
        }
      });
  
      if (!response.ok) {
        throw new Error(`Failed to fetch Google Doc. Status: ${response.status}`);
      }
  
      const buffer = await response.arrayBuffer();
      return new Uint8Array(buffer);
    } catch (error) {
      console.error('Error fetching Google Doc:', error);
      alert('Failed to fetch Google Doc for ' + assignment);
      throw new Error('Failed to fetch Google Doc');
    }
  }
  

  async getGoogleDriveFileMetadata(fileId: string): Promise<any> {
    this.initializeHeader();
    const url = `${environment.googleApiUrl}/drive/v3/files/${fileId}`;
  
    try {
      const response = await this.http.get(url, { headers: this.headers }).toPromise();
  
      if (!response) {
        throw new Error(`Failed to fetch metadata for file ${fileId}`);
      }
  
      return response;
    } catch (error) {
      console.error('Error fetching file metadata:', error);
      alert('Failed to fetch google drive file metadata');
      throw new Error('Failed to fetch file metadata');
    }
  }
  
}
