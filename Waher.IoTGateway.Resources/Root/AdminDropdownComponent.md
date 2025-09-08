{{

IsAuthorized(Privileges[]):=
(
	if !exists(User) || User is not Waher.Security.IUser then
		false
	else if count(Privileges)=0 then
		true
	else
		And([foreach Privilege in Privileges do (User.HasPrivilege(Privilege)???false)])
);
MenuItem(Text,Url,Privileges[]):=
(
	if IsAuthorized(Privileges) then
		]]<li><a href="((Url))">((Text))</a></li>
[[
);

MenuItem2(Text,Url,Privileges[]):=
(
	if IsAuthorized(Privileges) then
		]]<li><a href="((Url))">((Text))</a></li>[[ else ]]<div class="menuItemDisabled"><div class="menuItemContent">((Text))</div></div>
[[
);

SnifferItem(Text,Url,Privileges[]):=
(
	if IsAuthorized(Privileges) then
		]]<li><a onclick="OpenSniffer('((Url))')">((Text))</a></li>
[[
);

foreach Category in Categories DO (
    AtLeastOneAccecible:=false;
    foreach CategoryItem in Category.items do (
        if (IsAuthorized(CategoryItem.privilage)) THEN (
            AtLeastOneAccecible:=true;
        );
    );
    Category.visible:=AtLeastOneAccecible;
);
}}

<a href="/Admin.md">Admin</a>
<ul>
    {{
        foreach Category in Categories DO (
            if (Category.visible ?? false) THEN
            (
            ]]<li>
                <p>((Category.header))</p>
                <ul>[[;
                
                    foreach CategoryItem in Category.items DO (
                        if (exists(CategoryItem.sniffer) and CategoryItem.sniffer) then
                            SnifferItem(CategoryItem.label, CategoryItem.page, CategoryItem.privilage ?? "")
                        else
                            MenuItem(CategoryItem.label, CategoryItem.page, CategoryItem.privilage ?? "")
                    );
                ]]</ul>
            </li>[[;
            )       
        )
    }}
</ul>