<!DOCTYPE html>

<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>@ViewData("Title")</title>
    <link href="~/Content/jquery.datetimepicker.css" rel="stylesheet" />
    <script src="~/Scripts/jquery-2.1.3.min.js"></script>
</head>
<body>
    <div>
        @RenderBody()
    </div>

@Html.RenderScripts()
</body>
</html>
