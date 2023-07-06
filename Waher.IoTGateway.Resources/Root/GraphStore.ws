AuthenticateSession(Request,"User");
Authorize(User,"Admin.Graphs.Get");

if !exists(Posted) then BadRequest("No data posted.");

References:=select
	top Posted.maxCount *
from
	Waher.Script.Persistence.SPARQL.Sources.GraphReference
order by
	Created desc
offset
	Posted.offset;

DTMin:=System.DateTime.MinValue;

DateToStr(DT):=
(
	DT>DTMin ? DT.ToShortDateString() : ""
);

TimeToStr(DT):=
(
	DT>DTMin ? DT.ToLongTimeString() : ""
);

CanDelete:=User.HasPrivilege("Admin.Graphs.Delete");

[foreach Ref in Tokens do
{
	"graphUri": Ref.GraphUri,
	"creators": Concat(Ref.Creators??"",", "),
	"createdDate": DateToStr(Ref.Created),
	"createdTime": TimeToStr(Ref.Created),
	"updatedDate": DateToStr(Ref.Updated),
	"updatedTime": TimeToStr(Ref.Updated),
	"nrFiles": Ref.NrFiles,
	"canDelete": CanDelete
}]
