/// <reference types="@sveltejs/kit" />

declare namespace App {
	interface Locals {
		user?: import('$types').AuthUser;
	}
	interface PageData {}
	interface PageState {}
	interface Platform {}
}

interface ViewTransition {
	finished: Promise<void>;
	ready: Promise<void>;
	updateCallbackDone: Promise<void>;
}

interface Document {
	startViewTransition?(callback: () => Promise<void> | void): ViewTransition;
}
