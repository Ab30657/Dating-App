import {
	ChangeDetectionStrategy,
	Component,
	Input,
	OnInit,
	ViewChild,
} from '@angular/core';
import { Message } from 'src/app/_models/message';
import { MessageService } from '../../_services/message.service';
import { MembersService } from '../../_services/members.service';
import { NgForm } from '@angular/forms';

@Component({
	changeDetection: ChangeDetectionStrategy.OnPush,
	selector: 'app-member-messages',
	templateUrl: './member-messages.component.html',
	styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit {
	@Input() messages: Message[] = [];
	@Input() username: string;
	@ViewChild('messageForm') messageForm: NgForm;
	loading = false;

	messageContent: string;
	constructor(public messageService: MessageService) {}

	ngOnInit(): void {}

	sendMessage() {
		this.loading = true;
		this.messageService
			.sendMessage(this.username, this.messageContent)
			.then(() => {
				this.messageForm.reset();
			})
			.finally(() => (this.loading = false));
	}
}
