import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
	selector: 'app-photo-management',
	templateUrl: './photo-management.component.html',
	styleUrls: ['./photo-management.component.css'],
})
export class PhotoManagementComponent implements OnInit {
	photos: Photo[];
	constructor(private adminService: AdminService) {}

	ngOnInit(): void {
		this.getUnapprovedPhotos();
	}

	getUnapprovedPhotos() {
		this.adminService.getPhotosForApproval().subscribe((p) => {
			this.photos = p;
		});
	}

	approvePhoto(id) {
		this.adminService.approvePhoto(id).subscribe(() => {
			this.photos.splice(
				this.photos.findIndex((x) => x.id === id),
				1
			);
		});
	}

	rejectPhoto(id) {
		this.adminService.rejectPhoto(id).subscribe(() => {
			this.photos.splice(
				this.photos.findIndex((x) => x.id == id),
				1
			);
		});
	}
}
