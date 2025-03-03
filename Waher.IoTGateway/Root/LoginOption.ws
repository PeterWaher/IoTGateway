if !(Posted matches 
{
    "method": Required(String(PMethod))
}
)then BadRequest("Invalid posted data.");

C:=Create(HTTP.Cookie, "login-method", PMethod, Domain, "/", 3600 * 24 * 7, true, true);
Response.SetCookie(C);