<h2 id="native-popup-label">Prompt</h2>
<div id="native-popup-description">{Args.Message}</div>
<div id="native-popup-options">
<input id="native-prompt-input"/>
<button onclick="Popup.PromptSubmit(document.getElementById('native-prompt-input').value)">Submit</button>
<button onclick="Popup.PromptSubmit(null)">Cancel</button>
</div>