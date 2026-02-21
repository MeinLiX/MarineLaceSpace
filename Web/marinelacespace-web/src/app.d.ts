/// <reference types="@sveltejs/kit" />

declare namespace App {
	interface Locals {
		user?: import('$types').AuthUser;
	}
	interface PageData {}
	interface PageState {}
	interface Platform {}
}
