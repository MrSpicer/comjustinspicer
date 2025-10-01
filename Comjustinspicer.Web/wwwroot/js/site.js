// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Bulma
document.addEventListener('DOMContentLoaded', () => {

	// Get all "navbar-burger" elements
	const $navbarBurgers = Array.prototype.slice.call(document.querySelectorAll('.navbar-burger'), 0);

	// Add a click event on each of them
	$navbarBurgers.forEach(el => {
		el.addEventListener('click', () => {

			// Get the target from the "data-target" attribute
			const target = el.dataset.target;
			const $target = document.getElementById(target);

			// Toggle the "is-active" class on both the "navbar-burger" and the "navbar-menu"
			el.classList.toggle('is-active');
			$target.classList.toggle('is-active');

		});
	});

});
//end bulma

//CKEditor
const {
	ClassicEditor,
	Essentials,
	Bold,
	Italic,
	Underline,
	Strikethrough,
	Code,
	BlockQuote,
	Heading,
	Link,
	List,
	Indent,
	IndentBlock,
	Paragraph,
	Font,
	FontSize,
	FontFamily,
	FontColor,
	FontBackgroundColor,
	Alignment,
	Table,
	TableToolbar,
	CodeBlock,
	HorizontalLine,
	SpecialCharacters,
	RemoveFormat,
	Highlight,
	SourceEditing,
	HtmlEmbed
} = CKEDITOR;

//todo: licenseKey needs to move!
//todo: source edit doesn't wok very well. fix or find way to disable CKEditor for source editing
ClassicEditor
	.create(document.querySelector('#editor'), {
		licenseKey: 'eyJhbGciOiJFUzI1NiJ9.eyJleHAiOjE3NjAzOTk5OTksImp0aSI6IjI4MDYxN2JjLTYzZTYtNDJmOS04NzMzLWU5NDJlZGE3OTA0YSIsInVzYWdlRW5kcG9pbnQiOiJodHRwczovL3Byb3h5LWV2ZW50LmNrZWRpdG9yLmNvbSIsImRpc3RyaWJ1dGlvbkNoYW5uZWwiOlsiY2xvdWQiLCJkcnVwYWwiLCJzaCJdLCJ3aGl0ZUxhYmVsIjp0cnVlLCJsaWNlbnNlVHlwZSI6InRyaWFsIiwiZmVhdHVyZXMiOlsiKiJdLCJ2YyI6IjY1Y2YzNjZkIn0.2vMMmO9Luc_YFq6pmwB7Fv8FMhY170J2eIdfSXHFlwPj0B7idS1mV8SbWDSaSzdsEJTIj2TnrttioWEeliVVtg',
		plugins: [
			Essentials, Bold, Italic, Underline, Strikethrough, Code, BlockQuote,
			Heading, Link, List, Indent, IndentBlock, Paragraph,
			Font, FontSize, FontFamily, FontColor, FontBackgroundColor, Alignment,
			Table, TableToolbar, CodeBlock, HorizontalLine, SpecialCharacters,
			RemoveFormat, Highlight, SourceEditing, HtmlEmbed
		],
		toolbar: [
			'htmlEmbed',
			'undo', 'redo', '|',
			'heading', '|',
			'bold', 'italic', 'underline', 'strikethrough', 'removeFormat', '|',
			'fontSize', 'fontFamily', 'fontColor', 'fontBackgroundColor', 'highlight', '|',
			'link', 'bulletedList', 'numberedList', 'indent', '|',
			'blockQuote', 'code', 'codeBlock', '|',
			'insertTable', 'horizontalLine', '|',
			'specialCharacters', '|',
			'alignment', '|',
			'undo', 'redo', '|',
			'sourceEditing'
		],
		table: {
			contentToolbar: [ 'tableColumn', 'tableRow', 'mergeTableCells' ]
		}
	})
	.catch( 
		error => console.log(error)
	);

//end CKEditor