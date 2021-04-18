import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
	selector: 'app-register',
	templateUrl: './register.component.html',
	styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
	model: any = {};
	constructor(
		private accountService: AccountService,
		private toastr: ToastrService
	) {}
	@Output() cancelRegister = new EventEmitter();
	ngOnInit(): void {}

	register() {
		this.accountService.register(this.model).subscribe(
			(response) => {
				console.log(response);
				this.cancel();
			},
			(error) => {
				this.toastr.error(error.error);
			}
		);
	}

	cancel() {
		this.cancelRegister.emit(false);
	}
}
