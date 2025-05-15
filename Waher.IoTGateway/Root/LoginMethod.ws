if !(Posted matches 
{
    "method": Required(String(PMethod))
}
)then BadRequest("Invalid posted data.");

C:=Create(HTTP.Cookie, "login-method", PMethod, Split(Domain, ":")[0], "/", 60 * 60 * 24 * 365, true, true);
Response.SetCookie(C);