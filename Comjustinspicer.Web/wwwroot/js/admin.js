
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
					contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
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