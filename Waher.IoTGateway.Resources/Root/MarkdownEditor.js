﻿function MarkdownEditorBold(Button)
{
	ToggleInlineFormat(Button, "**");
}

function MarkdownEditorItalic(Button)
{
	ToggleInlineFormat(Button, "*");
}

function MarkdownEditorUnderline(Button)
{
	ToggleInlineFormat(Button, "_");
}

function MarkdownEditorTabKey(Button)
{
	InsertText(Button, "\t");
}

function MarkdownEditorStrikeThrough(Button)
{
	ToggleInlineFormat(Button, "~");
}

function MarkdownEditorSuperscript(Button)
{
	ToggleInlineFormat(Button, "^(", ")");
}

function MarkdownEditorSubscript(Button)
{
	ToggleInlineFormat(Button, "[", "]");
}

function MarkdownEditorInsert(Button)
{
	ToggleInlineFormat(Button, "__");
}

function MarkdownEditorDelete(Button)
{
	ToggleInlineFormat(Button, "~~");
}

function MarkdownEditorInlineCode(Button)
{
	ToggleInlineFormat(Button, "`");
}

function MarkdownEditorHeader1(Button)
{
	ToggleLineFormat(Button, "# ");
}

function MarkdownEditorHeader2(Button)
{
	ToggleLineFormat(Button, "## ");
}

function MarkdownEditorHeader3(Button)
{
	ToggleLineFormat(Button, "### ");
}

function MarkdownEditorHeader4(Button)
{
	ToggleLineFormat(Button, "#### ");
}

function MarkdownEditorHeader5(Button)
{
	ToggleLineFormat(Button, "##### ");
}

function MarkdownEditorBulletList(Button)
{
	ToggleBlockFormat(Button, "*\t", "\t");
}

function MarkdownEditorNumberList(Button)
{
	ToggleBlockFormat(Button, "#.\t", "\t");
}

function MarkdownEditorTaskList(Button)
{
	ToggleBlockFormat(Button, "[ ]\t", "\t");
}

function MarkdownEditorQuote(Button)
{
	ToggleBlockFormat(Button, ">\t", ">\t");
}

function MarkdownEditorAddedBlock(Button)
{
	ToggleBlockFormat(Button, "+>\t", "+>\t");
}

function MarkdownEditorDeletedBlock(Button)
{
	ToggleBlockFormat(Button, "->\t", "->\t");
}

function MarkdownEditorCodeBlock(Button)
{
	ToggleBlockFormat(Button, "\t", "\t", "");
}

function MarkdownEditorLeftAlignment(Button)
{
	ToggleBlockFormat(Button, "<<", "<<");
}

function MarkdownEditorCenterAlignment(Button)
{
	ToggleBlockFormat(Button, ">>", ">>", "<<");
}

function MarkdownEditorRightAlignment(Button)
{
	ToggleBlockFormat(Button, "", "", ">>");
}

function MarkdownEditorMarginAlignment(Button)
{
	ToggleBlockFormat(Button, "<<", "<<", ">>");
}

function MarkdownEditorLink(Button)
{
	ToggleInlineFormat(Button, "[", "]()", 2);
}

function MarkdownEditorImage(Button)
{
	ToggleInlineFormat(Button, "![", "]()", 2);
}

function MarkdownEditorAbbreviation(Button)
{
	ToggleInlineFormat(Button, "[", "](abbr:)", 7);
}

function MarkdownEditorHashTag(Button)
{
	ToggleInlineFormat(Button, "#", "");
}

function MarkdownEditorHorizontalSeparator(Button)
{
	ToggleInlineFormat(Button, "\n\n----------------------\n\n", "");
}

function MarkdownEditorDefinitionList(Button)
{
	ToggleInlineFormat(Button, "", "\n:\t\n", 3);
}

function MarkdownEditorTable(Button, Columns, Rows)
{
	var ColChars = 80 / Columns - 2;
	var ColFiller = ' '.repeat(ColChars);
	var Header = '-'.repeat(ColChars);
	var s = "\n|";
	var x, y;

	for (x = 0; x < Columns; x++)
		s += ColFiller + "|";

	s += "\n|";

	for (x = 0; x < Columns; x++)
		s += Header + "|";

	for (y = 0; y < Rows; y++)
	{
		s += "\n|";

		for (x = 0; x < Columns; x++)
			s += ColFiller + "|";
	}

	s += "\n";

	ToggleInlineFormat(Button, s, "");

	var Loop = Button.parentNode;
	while (Loop && Loop.tagName !== "TABLE")
		Loop = Loop.parentNode;

	Loop.setAttribute("style", "display:none");
	window.setTimeout(function () { Loop.setAttribute("style", ""); }, 100);
}

function ClosePreview(TextArea)
{
	var Prev = TextArea.previousSibling;
	if (Prev && Prev.tagName === "DIV" && Prev.className === "MarkdownPreview")
	{
		TextArea.parentNode.removeChild(Prev);
		TextArea.setAttribute("data-preview", "");
		TextArea.setAttribute("style", "");
		RaiseOnInput(TextArea);
	}
}

function MarkdownKeyDown(Control, Event)
{
	if (MarkdonwEditorKeyDownIsTabNavigation(Control, Event))    
        return true;

	if (!MarkdownEditorKeyDown(Control, Event))
		return false
}

function ToggleSidePreview(Button, on)
{
	var Div = GetTextArea(Button).parentElement
	var prev = Div.hasAttribute("data-preview-side")
	if (on)
		Div.setAttribute("data-preview-side", "")
	else
		Div.removeAttribute("data-preview-side")

	return prev !== on
}

function GetPreviewMode(Button)
{
	const container = GetTextArea(Button).parentElement;
	const onlyPreview = container.querySelector(".MarkdownEditorPreview")
	const bottomPreview = container.querySelector(".MarkdownEditorBottomPreviewAndEdit")
	const sidePreview = container.querySelector(".MarkdownEditorSidePreviewAndEdit")

	if (onlyPreview.hasAttribute("data-toggled"))
		return 1
	if (bottomPreview.hasAttribute("data-toggled"))
		return 2
	if (sidePreview.hasAttribute("data-toggled"))
		return 3

	return 0
}

function UpdatePreviewButtonsHtml(Button, indexToggled)
{
	const container = GetTextArea(Button).parentElement;
	const onlyPreview = container.querySelector(".MarkdownEditorPreview")
	const bottomPreview = container.querySelector(".MarkdownEditorBottomPreviewAndEdit")
	const sidePreview = container.querySelector(".MarkdownEditorSidePreviewAndEdit")

	const previews = [onlyPreview, bottomPreview, sidePreview]
	previews.forEach((e, i) => {
		if (i + 1 === indexToggled)
		{
			e.setAttribute("data-toggled", "")
		} else 
		{
			e.removeAttribute("data-toggled")
		}
	})
}

function ResetPreview(Button)
{
	var TextArea = GetTextArea(Button);
	const mode = GetPreviewMode(Button)

	if (mode === 1)
	{
		TextArea.style.display = "inline-block"
		TextArea.nextSibling.style.display = "none"
		TextArea.parentElement.removeAttribute("data-preview-side")	
		TextArea.setAttribute("data-preview", "")
	}
	else if (mode === 2)
		{
		TextArea.nextSibling.style.display = "none"
		TextArea.setAttribute("data-preview", "")
	}
	else if (mode === 3)
		{
		TextArea.nextSibling.style.display = "none"
		TextArea.parentElement.removeAttribute("data-preview-side")	
		TextArea.setAttribute("data-preview", "")
	}

	UpdatePreviewButtonsHtml(Button, 0)
}

function MarkdownEditorBottomPreviewAndEdit(Button)
{
	const TextArea = GetTextArea(Button);

	const mode = GetPreviewMode(Button)
	ResetPreview(Button)
	if (mode !== 2)
	{
		TextArea.setAttribute("data-preview", "true")
		TextArea.nextSibling.style.display = "inline-block"
		TryUpdatePreview(Button, true)
		UpdatePreviewButtonsHtml(Button, 2)
	}		
}

function MarkdownEditorSidePreviewAndEdit(Button)
{
	const TextArea = GetTextArea(Button);

	const mode = GetPreviewMode(Button)
	ResetPreview(Button)
	if (mode !== 3)
	{
		TextArea.setAttribute("data-preview", "true")
		TextArea.parentElement.setAttribute("data-preview-side", "")
		TextArea.nextSibling.style.display = "inline-block"
		TryUpdatePreview(Button, true)
		UpdatePreviewButtonsHtml(Button, 3)
	}		
}

function MarkdownEditorPreview(Button)
{
	const TextArea = GetTextArea(Button);

	const mode = GetPreviewMode(Button)
	ResetPreview(Button)
	if (mode !== 1)
	{
		TextArea.setAttribute("data-preview", "true")
		TextArea.style.display = "none"
		TextArea.nextSibling.style.display = "inline-block"
		TryUpdatePreview(Button, true)
		UpdatePreviewButtonsHtml(Button, 1)
	}		
}

function TryUpdatePreview(Button, ShowEditor)
{
	var TextArea = GetTextArea(Button);
	const PreviewDiv = TextArea.nextSibling;

	if (TextArea.getAttribute("data-preview"))
	{
		if (!ShowEditor)
		{
			RaiseOnInput(TextArea);
			return;
		}
	}

	if (TextArea.getAttribute("data-preview") !== "true")
		return

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			PreviewDiv.innerHTML = xhttp.responseText;
			TextArea.parentNode.appendChild(PreviewDiv, TextArea);
		};
	}

	xhttp.open("POST", "/MarkdownLab/MarkdownLabHtml.ws", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("Accept", "text/html");
	xhttp.send(TextArea.value || " ");
}

function TabNavigationOn(Button)
{
	const container = GetTextArea(Button).parentElement;
	const tabNavigation = container.querySelector(".MarkdownEditorTabNavigation")

	return tabNavigation.hasAttribute("data-toggled")
}

function MarkdownEditorToggleTabNavigation(Control)
{
	const container = GetTextArea(Control).parentElement;
	const button = container.querySelector(".MarkdownEditorTabNavigation")
	button.toggleAttribute("data-toggled")
}

function GetTextArea(Button, HidePreview)
{
	if (Button.tagName === "TEXTAREA")
		return Button;

	if (HidePreview === undefined)
		HidePreview = false;

	var PreviewFound = null;
	var Parent = GetToolbar(Button).parentNode;
	var Loop = Parent.firstChild;

	while (Loop && Loop.tagName !== "TEXTAREA")
	{
		var Next = Loop.nextSibling;

		if (Loop.tagName === "DIV" && Loop.className === "MarkdownPreview")
			PreviewFound = Loop;

		Loop = Next;
	}

	if (PreviewFound && Loop)
	{
		var Style = window.getComputedStyle(Loop);

		if (Style.display === "none" || HidePreview)
		{
			Parent.removeChild(PreviewFound);
			Loop.setAttribute("style", "");
		}
		else
			InitMarkdownEditorPreview(Loop);
	}

	return Loop;
}

function GetToolbar(Button)
{
	var Loop = Button.parentNode;

	while (Loop && (Loop.tagName !== "DIV" || Loop.className !== "MarkdownEditorToolbar"))
		Loop = Loop.parentNode;

	return Loop;
}

function ToggleInlineFormat(Button, LeftDelimiter, RightDelimiter, InsertOffset)
{
	if (RightDelimiter === undefined)
		RightDelimiter = LeftDelimiter;

	var LeftDelimiterLen = LeftDelimiter.length;
	var RightDelimiterLen = RightDelimiter.length;
	var TextArea = GetTextArea(Button);
	var s = TextArea.value;
	var SelectionStart;
	var SelectionEnd;

	if (TextArea.selectionStart !== undefined)
	{
		var Start = TextArea.selectionStart;
		var End = TextArea.selectionEnd;

		if (End < Start)
		{
			var Temp = Start;
			Start = End;
			End = Temp;
		}

		if (Start >= LeftDelimiterLen && s.substring(Start - LeftDelimiterLen, Start) === LeftDelimiter &&
			End <= s.length - RightDelimiterLen && s.substring(End, End + RightDelimiterLen) === RightDelimiter)
		{
			SetTextAreaValue(TextArea, s.substring(0, Start - LeftDelimiterLen) + s.substring(Start, End) + s.substring(End + RightDelimiterLen, s.length));
			SelectionStart = Start - LeftDelimiterLen;
			SelectionEnd = End - LeftDelimiterLen;
			InsertOffset = 0;
		}
		else
		{
			SetTextAreaValue(TextArea, s.substring(0, Start) + LeftDelimiter + s.substring(Start, End) + RightDelimiter + s.substring(End, s.length));
			SelectionStart = Start + LeftDelimiterLen;
			SelectionEnd = End + LeftDelimiterLen;
		}
	}
	else
	{
		SetTextAreaValue(TextArea, LeftDelimiter + s + RightDelimiter);
		SelectionStart = LeftDelimiterLen;
		SelectionEnd = s.length + LeftDelimiterLen;
	}

	if (InsertOffset)
	{
		SelectionEnd += InsertOffset;
		SelectionStart = SelectionEnd;
	}

	TextArea.setSelectionRange(SelectionStart, SelectionEnd);

	TextArea.focus();
}

function InsertText(Button, text)
{
	var TextArea = GetTextArea(Button);

	const Start = TextArea.selectionStart;
	const End = TextArea.selectionEnd

	var Start0 = TextArea.selectionStart;
	var End0 = TextArea.selectionEnd;

	if (End0 < Start0)
	{
		var Temp = Start0;
		Start0 = End0;
		End0 = Temp;
	}

	const CurrentText = TextArea.value;
	SetTextAreaValue(TextArea, CurrentText.substring(0, Start) + text + CurrentText.substring(End, CurrentText.length));

	if (End === Start)
		TextArea.setSelectionRange(End0 + text.length, End0 + text.length)
	else
		TextArea.setSelectionRange(Start0, End0 + text.length)
}

function ToggleLineFormat(Button, LeftDelimiter, RightDelimiter)
{
	if (RightDelimiter === undefined)
		RightDelimiter = "";

	var LeftDelimiterLen = LeftDelimiter.length;
	var RightDelimiterLen = RightDelimiter.length;
	var TextArea = GetTextArea(Button);
	var s = TextArea.value;

	if (TextArea.selectionStart !== undefined)
	{
		var Start0 = TextArea.selectionStart;
		var End0 = TextArea.selectionEnd;

		if (End0 < Start0)
		{
			var Temp = Start0;
			Start0 = End0;
			End0 = Temp;
		}

		var Start = FindStartOfLine(s, Start0);
		var End = FindEndOfLine(s, End0);

		if (s.substring(Start, Start + LeftDelimiterLen) === LeftDelimiter &&
			End <= s.length - RightDelimiterLen && s.substring(End, End + RightDelimiterLen) === RightDelimiter)
		{
			SetTextAreaValue(TextArea, s.substring(0, Start) + s.substring(Start + LeftDelimiterLen, End) + s.substring(End + RightDelimiterLen, s.length));
			TextArea.setSelectionRange(Start0 - LeftDelimiterLen, End0 - LeftDelimiterLen);
		}
		else
		{
			SetTextAreaValue(TextArea, s.substring(0, Start) + LeftDelimiter + s.substring(Start, End) + RightDelimiter + s.substring(End, s.length));
			TextArea.setSelectionRange(Start0 + LeftDelimiterLen, End0 + LeftDelimiterLen);
		}
	}
	else
	{
		SetTextAreaValue(TextArea, LeftDelimiter + s + RightDelimiter);
		TextArea.setSelectionRange(LeftDelimiterLen, s.length + LeftDelimiterLen);
	}

	TextArea.focus();
}

function FindStartOfLine(s, Position)
{
	var ch;

	while (Position > 0 && (ch = s[Position - 1]) !== '\n' && ch !== '\r')
		Position--;

	return Position;
}

function FindEndOfLine(s, Position)
{
	var LenMinus1 = s.length - 1;
	var ch;

	while (Position < LenMinus1 && (ch = s[Position + 1]) !== '\n' && ch !== '\r')
		Position++;

	return Position;
}

function SkipToNextLine(s, Position)
{
	var LenMinus1 = s.length - 1;

	if (Position < LenMinus1 && s[Position + 1] == '\r' && s[Position + 2] == '\n')
		return Position + 3;
	else if (Position < LenMinus1 && s[Position + 1] == '\n' && s[Position + 2] == '\r')
		return Position + 3;
	if (Position <= LenMinus1 && (s[Position + 1] == '\r' || s[Position + 1] == '\n'))
		return Position + 2;
	else
		return Position + 1;
}

function ToggleBlockFormat(Button, LeftDelimiter, LeftDelimiterRow2, RightDelimiter)
{
	if (RightDelimiter === undefined)
		RightDelimiter = "";

	var LeftDelimiterLen = LeftDelimiter.length;
	var RightDelimiterLen = RightDelimiter.length;
	var TextArea = GetTextArea(Button);
	var s = TextArea.value;

	if (TextArea.selectionStart !== undefined)
	{
		var Start0 = TextArea.selectionStart;
		var End0 = TextArea.selectionEnd;

		if (End0 < Start0)
		{
			var Temp = Start0;
			Start0 = End0;
			End0 = Temp;
		}
	}
	else
	{
		Start0 = 0;
		End0 = s.length;
	}

	if (Start0 == End0)
	{
		while (End0 > 0 && IsNewLine(s, End0) && IsNewLine(s, End0 - 1))
		{
			Start0--;
			End0--;
		}
	}

	var Start = FindStartOfBlock(s, Start0);
	var End = End0 >= s.length && End0 > 0 ? s.length - 1 : End0 > 0 && IsEndOfBlock(s, End0) ? End0 - 1 : FindEndOfBlock(s, End0);
	var First = true;

	if (End < Start)
	{
		s = s.substring(0, Start) + LeftDelimiter + RightDelimiter + s.substring(Start, s.length);
		Start0 += LeftDelimiterLen;
		End0 += LeftDelimiterLen;
	}
	else
	{
		var LeftDiff = 0;
		var RightDiff = 0;
		var RightCursorDiff = 0;
		var LeftCursorDiff = 0;
		var Diff;

		while (Start <= End)
		{
			var EndRow = FindEndOfLine(s, Start);
			var DiffSign;
			var LeftCursorOffset = LeftDelimiterLen;
			var RightCursorOffset = RightDelimiterLen;

			if (s.substring(Start, Start + LeftDelimiterLen) === LeftDelimiter &&
				EndRow <= s.length - RightDelimiterLen + 1 && s.substring(EndRow + 1 - RightDelimiterLen, EndRow + 1) === RightDelimiter)
			{
				s = s.substring(0, Start) + s.substring(Start + LeftDelimiterLen, EndRow + 1 - RightDelimiterLen) + s.substring(EndRow + 1, s.length);

				LeftCursorOffset = Start0 - Start;
				if (LeftCursorOffset < 0)
					LeftCursorOffset = 0;
				else if (LeftCursorOffset > LeftDelimiterLen)
					LeftCursorOffset = LeftDelimiterLen;

				RightCursorOffset = End0 + RightDelimiterLen - EndRow - 1;
				if (RightCursorOffset < 0)
					RightCursorOffset = 0;
				else if (RightCursorOffset > RightDelimiterLen)
					RightCursorOffset = RightDelimiterLen;

				DiffSign = -1;
			}
			else
			{
				s = s.substring(0, Start) + LeftDelimiter + s.substring(Start, EndRow + 1) + RightDelimiter + s.substring(EndRow + 1, s.length);
				DiffSign = 1;
			}

			LeftDiff = DiffSign * LeftDelimiterLen;
			LeftCursorDiff = DiffSign * LeftCursorOffset;

			RightDiff = DiffSign * RightDelimiterLen;
			RightCursorDiff = DiffSign * RightCursorOffset;

			if (Start0 >= Start)
			{
				if (Start0 >= EndRow + 1)
				{
					Start0 += RightDiff;
					if (Start0 < 0)
						Start0 = 0;
				}
				else if (Start0 >= EndRow + 1 + RightCursorDiff)
				{
					Start0 += RightCursorDiff;
					if (Start0 < 0)
						Start0 = 0;
				}

				Start0 += LeftCursorDiff;
			}

			if (End0 >= Start)
			{
				if (End0 >= EndRow + 1)
				{
					End0 += RightDiff;
					if (End0 < 0)
						End0 = 0;
				}
				else if (End0 >= EndRow + 1 + RightCursorDiff)
				{
					End0 += RightCursorDiff;
					if (End0 < 0)
						End0 = 0;
				}

				End0 += LeftCursorDiff;
			}
			else if (End0 >= Start + LeftCursorDiff)
			{
				End0 += LeftCursorDiff;
				if (End0 < 0)
					End0 = 0;
			}

			End += LeftDiff + RightDiff;
			EndRow += LeftDiff + RightDiff;

			Start = SkipToNextLine(s, EndRow);

			if (First)
			{
				First = false;
				LeftDelimiter = LeftDelimiterRow2;
				LeftDelimiterLen = LeftDelimiter.length;
			}
		}
	}

	SetTextAreaValue(TextArea, s);
	TextArea.setSelectionRange(Start0, End0);

	TextArea.focus();
}

function IsEndOfBlock(s, End)
{
	if (!IsNewLine(s, End))
		return false;

	End = SkipToNextLine(s, End - 1);
	return IsNewLine(s, End);
}

function IsNewLine(s, Start, End)
{
	if (End === undefined)
		return s[Start] === '\n' || s[Start] === '\r';
	else
	{
		var Len = End - Start;

		switch (Len)
		{
			case 1:
				s = s.substring(Start, End);
				return s === "\r" || s === "\n";

			case 2:
				s = s.substring(Start, End);
				return s === "\r\n" || s === "\n\r";

			default:
				return false;
		}
	}
}

function FindStartOfBlock(s, Position)
{
	var StartLine = FindStartOfLine(s, Position);
	while (StartLine > 0)
	{
		var StartPrevLine = FindStartOfLine(s, StartLine - 1);
		if (IsNewLine(s, StartPrevLine, StartLine))
			return StartLine;
		else
			StartLine = StartPrevLine;
	}

	return StartLine;
}


function FindEndOfBlock(s, Position)
{
	var LenMinus1 = s.length - 1;
	var EndLine = FindEndOfLine(s, Position);
	while (EndLine < LenMinus1)
	{
		var StartNextLine = SkipToNextLine(s, EndLine);
		if (IsNewLine(s, StartNextLine))
			return EndLine;

		EndLine = FindEndOfLine(s, StartNextLine);
	}

	return EndLine;
}

function SetTextAreaValue(TextArea, Value)
{
	// TODO: Update TextArea in such a way that intermediate edits are available in undo-list.
	TextArea.value = Value;
	RaiseOnInput(TextArea);
}

function RaiseOnInput(TextArea)
{
	try
	{
		var Event = new InputEvent('input',
			{
				bubbles: true,
				cancelable: true,
			});

		TextArea.dispatchEvent(Event);
	}
	catch (e)
	{
		console.log(e);
	}
}

function MarkdonwEditorKeyDownIsTabNavigation(Control, Event)
{
	return Event.keyCode === 9 && TabNavigationOn(Control)		
}

function MarkdownEditorKeyDown(Control, Event)
{
	if (Control.getAttribute("data-preview"))
		InitMarkdownEditorPreview(Control);

	if (Event.altKey && !Event.ctrlKey && !Event.shiftKey)
	{
		switch (Event.keyCode)
		{
			case 37:	// ALT+LEFT
				MarkdownEditorLeftAlignment(Control);
				return false;

			case 39:	// ALT+RIGHT
				MarkdownEditorRightAlignment(Control);
				return false;

			case 40:	// ALT+DOWN
				MarkdownEditorCenterAlignment(Control);
				return false;

			case 38:	// ALT+UP
				MarkdownEditorMarginAlignment(Control);
				return false;

			case 49:	// ALT+1
				MarkdownEditorPreview(Control);
				return false;

			case 50:	// ALT+2
				MarkdownEditorBottomPreviewAndEdit(Control);
				return false;

			case 84:	// ALT+T
				MarkdownEditorToggleTabNavigation(Control);
				return false
		}
	}
	else if (Event.ctrlKey && !Event.altKey)
	{
		if (Event.shiftKey)
		{
			switch (Event.keyCode)
			{
				case 50:	// CTRL+SHIFT+2
					MarkdownEditorQuote(Control);
					return false;

				case 45:	// CTRL+SHIFT+INS
					MarkdownEditorAddedBlock(Control);
					return false;

				case 46:	// CTRL+SHIFT+DEL
					MarkdownEditorDeletedBlock(Control);
					return false;

				case 71:	// CTRL+SHIFT+G
					MarkdownEditorCodeBlock(Control);
					return false;

				case 109:	// CTRL+-
					MarkdownEditorHorizontalSeparator(Control);
					return false;
			}
		}
		else
		{
			switch (Event.keyCode)
			{
				case 66:	// CTRL+B
					MarkdownEditorBold(Control);
					return false;

				case 73:	// CTRL+I
					MarkdownEditorItalic(Control);
					return false;

				case 85:	// CTRL+U
					MarkdownEditorUnderline(Control);
					return false;

				case 83:	// CTRL+S
					MarkdownEditorStrikeThrough(Control);
					return false;

				case 80:	// CTRL+P
					MarkdownEditorSuperscript(Control);
					return false;

				case 68:	// CTRL+D
					MarkdownEditorSubscript(Control);
					return false;

				case 45:	// CTRL+INS
					MarkdownEditorInsert(Control);
					return false;

				case 46:	// CTRL+DEL
					MarkdownEditorDelete(Control);
					return false;

				case 71:	// CTRL+G
					MarkdownEditorInlineCode(Control);
					return false;

				case 49:	// CTRL+1
					MarkdownEditorHeader1(Control);
					return false;

				case 50:	// CTRL+2
					MarkdownEditorHeader2(Control);
					return false;

				case 51:	// CTRL+3
					MarkdownEditorHeader3(Control);
					return false;

				case 52:	// CTRL+4
					MarkdownEditorHeader4(Control);
					return false;

				case 53:	// CTRL+5
					MarkdownEditorHeader5(Control);
					return false;

				case 106:	// CTRL+*
					MarkdownEditorBulletList(Control);
					return false;

				case 109:	// CTRL+-
					MarkdownEditorNumberList(Control);
					return false;

				case 220:	// CTRL+§
					MarkdownEditorDefinitionList(Control);
					return false;

				case 107:	// CTRL++
					MarkdownEditorTaskList(Control);
					return false;

				case 76:	// CTRL+L
					MarkdownEditorLink(Control);
					return false;

				case 77:	// CTRL+M
					MarkdownEditorImage(Control);
					return false;

				case 82:	// CTRL+R
					MarkdownEditorAbbreviation(Control);
					return false;
			}
		}
	}
	else if (!Event.altKey && !Event.ctrlKey && !Event.shiftKey)
	{
		switch (Event.keyCode)
		{
			case 112:	// F1
				MarkdownEditorHelp(Control);
				return false;
			case 9:
				MarkdownEditorTabKey(Control)
				return false
		}
	}

	return true;
}

function InitMarkdownEditorPreview(Control)
{
	var Timer = Control.getAttribute("data-previewtimer");
	if (Timer)
	{
		window.clearTimeout(Timer);
		Control.setAttribute("data-previewtimer", "");
	}

	Timer = window.setTimeout(function ()
	{
		Control.setAttribute("data-previewtimer", "");
		TryUpdatePreview(Control, true);
	}, 500);

	Control.setAttribute("data-previewtimer", Timer);
}

function MarkdownEditorHelp(Control)
{
	var Window = window.open("/Markdown.md", "_blank");
	Window.focus();
}

function InitializeMarkdownEditor(container)
{
	const textInput = container.nextElementSibling || container.parentElement.getElementsByTagName("TEXTAREA")[0]
	textInput.setAttribute("data-visible", "")
	container.appendChild(textInput)

	if (!textInput.onkeydown)
	{
		textInput.addEventListener("keydown", e => { MarkdownKeyDown(e.target, e) === false && e.preventDefault() })
	}
	
	
	if (container.getAttribute("data-scale") === "true")
	{
		textInput.addEventListener("input", e => {AdaptSize(e.target)})
		requestAnimationFrame(() => {
			AdaptSize(container.getElementsByTagName("textarea")[0])
		})
	}

	const preview = document.createElement("div")
	preview.classList.add("MarkdownPreview")
	preview.style.display = "none";
	container.appendChild(preview)
}

function MarkdownEditorInitializeHack(event, previewOn)
{
	const container = event.target.previousElementSibling
	event.target.remove()
	setTimeout(() => {
		InitializeMarkdownEditor(container)
		if (previewOn)
		{
			setTimeout(() => {
				MarkdownEditorBottomPreviewAndEdit(container.getElementsByClassName("MarkdownEditorBottomPreviewAndEdit")[0])
			}, 0)
		}
	}, 0)
}

function AdaptSize(Control)
{
	{
		var maxheight = Math.floor((("innerHeight" in window ? window.innerHeight : document.documentElement.offsetHeight) * 2) / 3);
		var h = Control.scrollHeight;
		if (h > maxheight)
			h = maxheight;

		if (h > Control.clientHeight)
			Control.style.height = h + "px";
	}
}

