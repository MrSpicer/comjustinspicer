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
	HtmlEmbed,
	GeneralHtmlSupport
} = CKEDITOR;

// CKEditor dynamic configuration (license key injected server-side)
(() => {
	const editorElement = document.querySelector('#editor');
	if (!editorElement) {
		return; // No editor on this page.
	}

	const ckLicenseKey = (window.__APP_CONFIG__ && window.__APP_CONFIG__.ckEditorLicenseKey)
		|| (document.querySelector('meta[name="ckeditor-license-key"]')?.content)
		|| '';

	if (!ckLicenseKey) {
		console.warn('CKEditor license key not configured. Proceeding without it.');
	}

	if (ckLicenseKey) {
	ClassicEditor
		.create(editorElement, {
			licenseKey: ckLicenseKey,
			plugins: [
				Essentials, Bold, Italic, Underline, Strikethrough, Code, BlockQuote,
				Heading, Link, List, Indent, IndentBlock, Paragraph,
				Font, FontSize, FontFamily, FontColor, FontBackgroundColor, Alignment,
				Table, TableToolbar, CodeBlock, HorizontalLine, SpecialCharacters,
				RemoveFormat, Highlight, SourceEditing, HtmlEmbed, GeneralHtmlSupport
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
			},
			htmlSupport: {
				allow: [
					{
						name: /.*/,
						attributes: true,
						classes: true,
						styles: true
					}
				]
			}
		})
		.catch(error => console.log(error));
	}
})();

//end CKEditor