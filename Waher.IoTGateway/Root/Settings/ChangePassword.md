Title: Change Password
Description: Allows a logged in user to change its password.
Date: 2023-07-11
Author: Peter Waher
Master: /Master.md
Javascript: /Settings/XMPP.js
UserVariable: User
Login: /Login.md

========================================================================

<form action="ChangePassword.md" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Change Password</legend>

{{
if exists(Posted) then
(
	CharStat(s):=
	(
		Result:=
		{
			"NrLetters":0,
			"NrDigits":0,
			"NrSymbols":0,
			"NrPunctuations":0,
			"NrSeparators":0,
			"NrUpperCase":0,
			"NrLowerCase":0,
			"NrControl":0,
			"NrSurrogates":0,
			"NrWhiteSpace":0
		};

		Char:=System.Char;

		foreach ch in s do
		(
			if Char.IsLetter(ch) then Result.NrLetters++;
			if Char.IsDigit(ch) then Result.NrDigits++;
			if Char.IsSymbol(ch) then Result.NrSymbols++;
			if Char.IsPunctuation(ch) then Result.NrPunctuations++;
			if Char.IsSeparator(ch) then Result.NrSeparators++;
			if Char.IsUpper(ch) then Result.NrUpperCase++;
			if Char.IsLower(ch) then Result.NrLowerCase++;
			if Char.IsControl(ch) then Result.NrControl++;
			if Char.IsSurrogate(ch) then Result.NrSurrogates++;
			if Char.IsWhiteSpace(ch) then Result.NrWhiteSpace++;
		);

		Result
	);

	PwdStat:=CharStat(Posted.Password);
	IsSufficientlyRandomBase64:=
	(
		Bin:=Base64Decode(Posted.Password);
		H:=Histogram([foreach x in Bin : x],0,256,12);
		Min(H[1])>0
	) ??? false;

	if Posted.Password!=Posted.Password2 then
		]]<div class='error'>New passwords do not match.</div>[[
	else if len(Posted.Password)<12 then
		]]<div class='error'>New password too short.</div>[[
	else if PwdStat.NrLetters<3 and !IsSufficientlyRandomBase64 then
		]]<div class='error'>At least three letters are required in the new password.</div>[[
	else if PwdStat.NrDigits<3 and !IsSufficientlyRandomBase64 then
		]]<div class='error'>At least three digits are required in the new password.</div>[[
	else if PwdStat.NrUpperCase<1 and !IsSufficientlyRandomBase64 then
		]]<div class='error'>You need at least an upper-case character in the new password.</div>[[
	else if PwdStat.NrLowerCase<1 and !IsSufficientlyRandomBase64 then
		]]<div class='error'>You need at least a lower-case character in the new password.</div>[[
	else if PwdStat.NrSymbols+PwdStat.NrPunctuations<3 and !IsSufficientlyRandomBase64 then
		]]<div class='error'>At least three symbols or punctuations are required in the new password.</div>[[
	else if PwdStat.NrControl+PwdStat.NrSurrogates+PwdStat.NrWhiteSpace>0 then
		]]<div class='error'>Avoid control characters, surrogates and white-space in new passwords.</div>[[
	else
	(
		WSU:=Waher.Security.Users;
		Result := WSU.Users.Login(User.UserName,Posted.CurrentPassword,Request.RemoteEndPoint,"Web");
		if Result.Type=WSU.LoginResultType.PermanentlyBlocked then
			]]<div class='error'>You've been permanently blocked. Operation blocked.</div>[[
		else if Result.Type=WSU.LoginResultType.TemporarilyBlocked then
			]]<div class='error'>Too many failed login attempts in a row registered. Try again after ((Result.Next)).</div>[[
		else if Result.Type=WSU.LoginResultType.InvalidCredentials then
			]]<div class='error'>Invalid current password.</div>[[
		else if Result.Type=WSU.LoginResultType.NoPassword then
			]]<div class='error'>Password not provided.</div>[[
		else if Result.Type=WSU.LoginResultType.Success then
		(
			try
			(
				Result.User.PasswordHash:=Base64Encode(WSU.Users.ComputeHash(User.UserName,Posted.Password));
				UpdateObject(Result.User);
				WSU.Users.ClearCache();

				LogInformational("Password changed by User, via Admin.", 
				{
					"Object":Result.User.UserName, 
					"Actor":User.UserName
				});

				]]<div class='result'>Password changed.</div>[[
			)
			catch
			(
				]]<div class='error'>((Exception.Message))</div>[[
			)
		)
	)
)
}}

<p>
<label for="CurrentPassword">Current Password:</label>  
<input type="password" id="CurrentPassword" name="CurrentPassword" required value='{{Posted?.CurrentPassword}}'/>
</p>

<p>
<label for="Password">New Password:</label>  
<input type="password" id="Password" name="Password" required value='{{Posted?.Password}}'/>
</p>

<p>
<label for="Password2">Repeat New Password:</label>  
<input type="password" id="Password2" name="Password2" required value='{{Posted?.Password2}}'/>
</p>

Rules for a new password:

* It must either be a *sufficiently random* BASE64-encoded binary string.
	* Press the *Create Random Password* below to generate such a password.
	* **Note**: Copy password before pressing Change Password (unless you have an exceptional memory). You will need it to login or change the password again.
* Or enter a password that:
	* Is at least *12 characters* long.
	* Contains at least *3 letters*.
		* Of which at least *1 is upper-case*.
		* Of which at least *1 is lower-case*.
	* Contains at least *3 digits*.
	* Contains at least *3 of punctuations or symbols*.
	* Contains *no* control characters, surrogates or white-space.

<button type='button' onclick='RandomizePassword()'>Create Random Password</button>
<button type="submit" class="posButton">Change Password</button>

</fieldset>
</form>
