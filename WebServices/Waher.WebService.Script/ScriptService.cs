using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Security;

namespace Waher.WebService.Script
{
    /// <summary>
    /// Web service that can be used to execute script on the server.
    /// </summary>
    public class ScriptService : HttpAsynchronousResource, IHttpPostMethod
    {
        private readonly HttpAuthenticationScheme[] authenticationSchemes;
        private readonly Dictionary<string, Expression> expressions = new Dictionary<string, Expression>();

        /// <summary>
        /// Web service that can be used to execute script on the server.
        /// </summary>
        /// <param name="ResourceName">Name of resource.</param>
        /// <param name="AuthenticationSchemes">Authentication schemes.</param>
        public ScriptService(string ResourceName, params HttpAuthenticationScheme[] AuthenticationSchemes)
            : base(ResourceName)
        {
            this.authenticationSchemes = AuthenticationSchemes;
        }

        /// <summary>
        /// If the resource handles sub-paths.
        /// </summary>
        public override bool HandlesSubPaths
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// If the resource uses user sessions.
        /// </summary>
        public override bool UserSessions
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// If the POST method is allowed.
        /// </summary>
        public bool AllowsPOST
        {
            get { return true; }
        }

        /// <summary>
        /// Executes the POST method on the resource.
        /// </summary>
        /// <param name="Request">HTTP Request</param>
        /// <param name="Response">HTTP Response</param>
        /// <exception cref="HttpException">If an error occurred when processing the method.</exception>
        public void POST(HttpRequest Request, HttpResponse Response)
        {
            if (Request.Session is null || !Request.Session.TryGetVariable("User", out Variable v) || !(v.ValueObject is IUser User))
                throw new ForbiddenException("Access denied.");

            object Obj = Request.HasData ? Request.DecodeData() : null;
            string s = Obj as string;
            string Tag = Request.Header["X-TAG"];

            if (string.IsNullOrEmpty(Tag))
                throw new BadRequestException();

            Variables Variables = new Variables();
            Request.Session.CopyTo(Variables);

            Variables["Request"] = Request;
            Variables["Response"] = Response;

            StringBuilder sb = new StringBuilder();
            Variables.ConsoleOut = new StringWriter(sb);

            Expression Exp = null;
            IElement Result;

            if (string.IsNullOrEmpty(s))
            {
                if (!string.IsNullOrEmpty(s = Request.Header["X-X"]) && int.TryParse(s, out int x) &&
                    !string.IsNullOrEmpty(s = Request.Header["X-Y"]) && int.TryParse(s, out int y))
                {
                    if (!(Variables["Graphs"] is Dictionary<string, KeyValuePair<Graph, object[]>> Graphs))
                        throw new NotFoundException("Graphs not found.");

                    KeyValuePair<Graph, object[]> Rec;

                    lock (Graphs)
                    {
                        if (!Graphs.TryGetValue(Tag, out Rec))
                            throw new NotFoundException("Graph not found.");
                    }

                    s = Rec.Key.GetBitmapClickScript(x, y, Rec.Value);

                    Response.ContentType = "text/plain";
                    Response.Write(s);
                    Response.SendResponse();
                }
                else
                {
                    lock (this.expressions)
                    {
                        if (!this.expressions.TryGetValue(Tag, out Exp))
                            throw new NotFoundException("Expression not found.");
                    }

                    Exp.Tag = Response;
                }
            }
            else
            {
                try
                {
                    Exp = new Expression(s);

                    if (!Exp.ForAll(this.IsAuthorized, User, false))
                        throw new ForbiddenException("Unauthorized to execute expression.");
                }
                catch (Exception ex)
                {
                    this.SendResponse(Variables, new ObjectValue(ex), null, Response, false);
                    return;
                }

                Exp.Tag = Response;

                lock (this.expressions)
                {
                    this.expressions[Tag] = Exp;
                }

                Exp.OnPreview += (sender, e) =>
                {
                    if (Exp.Tag is HttpResponse Response2 && !Response2.HeaderSent)
                        this.SendResponse(Variables, e.Preview, null, Response2, true);
                };

                Log.Notice("Script evaluated:\r\n\r\n" + s, this.ResourceName, User.UserName, "ScriptEval",
                    new KeyValuePair<string, object>("RemoteEndPoint", Request.RemoteEndPoint));

                Task.Run(() =>
                {
                    try
                    {
                        try
                        {
                            Result = Exp.Root.Evaluate(Variables);
                        }
                        catch (ScriptReturnValueException ex)
                        {
                            Result = ex.ReturnValue;
                        }
                        catch (Exception ex)
                        {
                            Result = new ObjectValue(ex);
                        }

                        if (Exp.Tag is HttpResponse Response2 && !Response2.HeaderSent)
                        {
                            lock (this.expressions)
                            {
                                this.expressions.Remove(Tag);
                            }

                            this.SendResponse(Variables, Result, sb, Response2, false);
                        }

                        Variables.CopyTo(Request.Session);
                    }
                    catch (Exception ex)
                    {
                        Log.Critical(ex);
                    }
                });
            }
        }

        private bool IsAuthorized(ref ScriptNode Node, object State)
        {
			if (Node is null)
				return true;
			else if (State is IUser User)
                return User.HasPrivilege(Node.GetType().FullName);
            else
                return false;
        }

        private void SendResponse(Variables Variables, IElement Result, StringBuilder sb, HttpResponse Response,
            bool More)
        {
            Variables["Ans"] = Result;

            object Obj;
            string s;

            if (Result is Graph G)
            {
                GraphSettings Settings = new GraphSettings();
                Tuple<int, int> Size;
                double d;

                if ((Size = G.RecommendedBitmapSize) != null)
                {
                    Settings.Width = Size.Item1;
                    Settings.Height = Size.Item2;

                    Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
                    Settings.MarginRight = Settings.MarginLeft;

                    Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
                    Settings.MarginBottom = Settings.MarginTop;
                    Settings.LabelFontSize = 12.0 * Settings.Height / 480;
                }
                else
                {
                    if (Variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
                    {
                        Settings.Width = (int)Math.Round(d);
                        Settings.MarginLeft = (int)Math.Round(15 * d / 640);
                        Settings.MarginRight = Settings.MarginLeft;
                    }
                    else if (!Variables.ContainsVariable("GraphWidth"))
                        Variables["GraphWidth"] = (double)Settings.Width;

                    if (Variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
                    {
                        Settings.Height = (int)Math.Round(d);
                        Settings.MarginTop = (int)Math.Round(15 * d / 480);
                        Settings.MarginBottom = Settings.MarginTop;
                        Settings.LabelFontSize = 12 * d / 480;
                    }
                    else if (!Variables.ContainsVariable("GraphHeight"))
                        Variables["GraphHeight"] = (double)Settings.Height;
                }

                using (SKImage Bmp = G.CreateBitmap(Settings, out object[] States))
                {
                    string Tag = Guid.NewGuid().ToString();
                    SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
                    byte[] Bin = Data.ToArray();
                    s = Convert.ToBase64String(Bin, 0, Bin.Length);
                    s = "<figure><img border=\"2\" width=\"" + Settings.Width.ToString() + "\" height=\"" + Settings.Height.ToString() +
                        "\" src=\"data:image/png;base64," + s + "\" onclick=\"GraphClicked(this,event,'" + Tag + "');\" /></figure>";

                    Data.Dispose();

                    if (!(Variables["Graphs"] is Dictionary<string, KeyValuePair<Graph, object[]>> Graphs))
                    {
                        Graphs = new Dictionary<string, KeyValuePair<Graph, object[]>>();
                        Variables["Graphs"] = Graphs;
                    }

                    lock (Graphs)
                    {
                        Graphs[Tag] = new KeyValuePair<Graph, object[]>(G, States);
                    }
                }
            }
            else if (Result.AssociatedObjectValue is SKImage Img)
            {
                SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100);
                byte[] Bin = Data.ToArray();

                s = Convert.ToBase64String(Bin, 0, Bin.Length);
                s = "<figure><img border=\"2\" width=\"" + Img.Width.ToString() + "\" height=\"" + Img.Height.ToString() +
                    "\" src=\"data:image/png;base64," + s + "\" /></figure>";

                Data.Dispose();
            }
            else if (Result.AssociatedObjectValue is Exception ex)
            {
                ex = Log.UnnestException(ex);

                if (ex is AggregateException ex2)
                {
                    StringBuilder sb2 = new StringBuilder();

                    foreach (Exception ex3 in ex2.InnerExceptions)
                    {
                        sb2.Append("<p><font style=\"color:red;font-weight:bold\"><code>");
                        sb2.Append(this.FormatText(XML.HtmlValueEncode(ex3.Message)));
                        sb2.Append("</code></font></p>");
                    }

                    s = sb2.ToString();
                }
                else
                    s = "<p><font style=\"color:red;font-weight:bold\"><code>" + this.FormatText(XML.HtmlValueEncode(ex.Message)) + "</code></font></p>";
            }
            else if (Result is ObjectMatrix M && M.ColumnNames != null)
            {
                StringBuilder Html = new StringBuilder();

                s = Result.ToString();

                Html.Append("<div class='clickable' onclick='SetScript(\"");
                Html.Append(s.ToString().Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"").Replace("'", "\\'"));
                Html.Append("\");'><table><thead><tr>");

                foreach (string s2 in M.ColumnNames)
                {
                    Html.Append("<th>");
                    Html.Append(this.FormatText(XML.HtmlValueEncode(s2)));
                    Html.Append("</th>");
                }

                Html.Append("</tr></thead><tbody>");

                int x, y;

                for (y = 0; y < M.Rows; y++)
                {
                    Html.Append("<tr>");

                    for (x = 0; x < M.Columns; x++)
                    {
                        Html.Append("<td>");

                        object Item = M.GetElement(x, y).AssociatedObjectValue;
                        if (Item != null)
                        {
                            if (Item is string s2)
                                Html.Append(this.FormatText(XML.HtmlValueEncode(s2)));
                            else
                                Html.Append(this.FormatText(XML.HtmlValueEncode(Expression.ToString(Item))));
                        }

                        Html.Append("</td>");
                    }

                    Html.Append("</tr>");
                }

                Html.Append("</tbody></table></div>");
                s = Html.ToString();
            }
            else
            {
                s = Result.ToString();
                s = "<div class='clickable' onclick='SetScript(\"" + s.ToString().Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"").Replace("'", "\\'") +
                    "\");'><p><font style=\"color:red\"><code>" + this.FormatText(XML.HtmlValueEncode(s)) + "</code></font></p></div>";
            }

            if (sb != null)
            {
                string s2 = sb.ToString();
                if (!string.IsNullOrEmpty(s2))
                    s = "<p><font style=\"color:blue\"><code>" + this.FormatText(XML.HtmlValueEncode(s2)) + "</code></font></p>" + s;
            }

            s = "{\"more\":" + CommonTypes.Encode(More) + ",\"html\":\"" + JSON.Encode(s) + "\"}";
            Response.ContentType = "application/json";
            Response.Write(s);
            Response.SendResponse();
            Response.Dispose();
        }

        private string FormatText(string s)
        {
            return s.Replace("\r\n", "\n").Replace("\n", "<br/>").Replace("\r", "<br/>").
                Replace("\t", "&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;");
        }

        /// <summary>
        /// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
        /// </summary>
        /// <param name="Request">Current request</param>
        public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
        {
            return this.authenticationSchemes;
        }
    }
}
