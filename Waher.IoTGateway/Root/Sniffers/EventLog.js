function NewEvent(Data)
{
    var TBody = document.getElementById("EventLogBody");

    var Tr = document.createElement("TR");
    Tr.setAttribute("class", Data.type);
    TBody.appendChild(Tr);

    Tr.appendChild(CreateTd(Data, Data.date));
    Tr.appendChild(CreateTd(Data, Data.time));
    Tr.appendChild(CreateTd(Data, Data.type));
    Tr.appendChild(CreateTd(Data, Data.level));
    Tr.appendChild(CreateTd(Data, Data.id));
    Tr.appendChild(CreateTd(Data, Data.object));
    Tr.appendChild(CreateTd(Data, Data.actor));
    Tr.appendChild(CreateTd(Data, Data.module));
    Tr.appendChild(CreateTd(Data, Data.facility));

    var Td = CreateTd(Data, Data.message);
    Tr.appendChild(Td);

    var Tags = Data.tags;
    var j, d = Tags.length;

    if (Tags != null && d > 0)
    {
        Table = document.createElement("TABLE");
        Table.setAttribute("style", "word-break:break-all");
        Td.appendChild(Table);

        THead = document.createElement("THEAD");
        Table.appendChild(THead);

        Tr = document.createElement("TR");
        THead.appendChild(Tr);

        Th = document.createElement("TH");
        Th.innerText = "Tag";
        Tr.appendChild(Th);

        Th = document.createElement("TH");
        Th.innerText = "Value";
        Tr.appendChild(Th);

        TBody = document.createElement("TBODY");
        Table.appendChild(TBody);

        for (j = 0; j < d; j++)
        {
            Tr = document.createElement("TR");
            TBody.appendChild(Tr);

            Td = document.createElement("TD");
            Td.innerText = Tags[j].name;
            Tr.appendChild(Td);

            Td = document.createElement("TD");
            Td.innerText = Tags[j].value;
            Tr.appendChild(Td);
        }
    }

    if (Data.stackTrace != null && Data.stackTrace.length > 0)
    {
        Tr = document.createElement("TR");
        Tr.className = Data.type;
        TBody.appendChild(Tr);

        Td = document.createElement("TD");
        Td.setAttribute("colspan", "10");
        Tr.appendChild(Td);

        var Pre = document.createElement("PRE");
        Td.appendChild(Pre);

        var Code = document.createElement("CODE");
        Code.innerText = Data.stackTrace;
        Pre.appendChild(Code);
    }
}

function CreateTd(Data, Text)
{
    var Td = document.createElement("TD");
    Td.innerText = Text;
    Td.setAttribute("class", Data.type);

    return Td;
}

function SinkClosed(Data)
{
    var TBody = document.getElementById("EventLogBody");
    var Tr = document.createElement("TR");
    Tr.setAttribute("class", "Notice");
    TBody.appendChild(Tr);

    var Td = document.createElement("TD");
    Td.setAttribute("colspan", "10");
    Tr.appendChild(Td);
    Td.innerText = "Sink closed.";
}