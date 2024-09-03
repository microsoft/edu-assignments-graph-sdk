import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-migrated-data',
  templateUrl: './migrated-data.component.html',
  styleUrl: './migrated-data.component.scss'
})
export class MigratedDataComponent {

  @Input() createdAssignment: any;
  @Input() createdModules: any;

  // ngOnInit(): void {
  //   this.createdAssignment = this.createdAssignment[0].split(',');
  //   this.createdModules = this.createdModules.split(',');
  // }
}
