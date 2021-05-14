import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';

// const httpOptions = {
// 	headers: new HttpHeaders({
// 		Authorization:
// 			'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token,
// 	}),
// };

@Injectable({
	providedIn: 'root',
})
export class MembersService {
	baseUrl = environment.apiUrl;
	members: Member[] = [];

	constructor(private http: HttpClient) {}

	getMembers(userParams: UserParams) {
		// if (this.members.length > 0) return of(this.members);
		let params = this.getPaginationHeaders(
			userParams.pageNumber,
			userParams.pageSize
		);
		params = params.append('minAge', userParams.minAge.toString());
		params = params.append('maxAge', userParams.maxAge.toString());
		params = params.append('gender', userParams.gender);
		return this.getPaginatedResult<Member[]>(
			this.baseUrl + 'users',
			params
		);
		// map((members) => {
		// 	this.members = members;
		// 	return members;
		// })
	}

	private getPaginatedResult<T>(url, params) {
		const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
		return this.http
			.get<T>(url, {
				observe: 'response',
				params,
			})
			.pipe(
				map((response) => {
					paginatedResult.result = response.body;
					if (response.headers.get('Pagination') !== null) {
						paginatedResult.pagination = JSON.parse(
							response.headers.get('Pagination')
						);
					}
					return paginatedResult;
				})
			);
	}

	private getPaginationHeaders(pageNumber: number, pageSize: number) {
		let params = new HttpParams();
		params = params.append('pageNumber', pageNumber.toString());
		params = params.append('pageSize', pageSize.toString());
		return params;
	}

	getMember(username: string) {
		const member = this.members.find((x) => x.username === username);
		if (member !== undefined) return of(member);
		return this.http.get<Member>(this.baseUrl + 'users/' + username);
	}

	updateMember(member: Member) {
		return this.http.put(this.baseUrl + 'users', member).pipe(
			map(() => {
				const index = this.members.indexOf(member);
				this.members[index] = member;
			})
		);
	}

	setMainPhoto(photoId: number) {
		return this.http.put(
			this.baseUrl + 'users/set-main-photo/' + photoId,
			{}
		);
	}

	deletePhotos(photoId: number) {
		return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
	}
}
