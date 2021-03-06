'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications
Option Explicit On
Option Strict On

Imports Microsoft.VisualBasic

Namespace WebApplication

	Public Module Extension

		Function ShortDescriptionFromText(ByVal Text As String) As String
      Dim StringBuilder As New System.Text.StringBuilder(Text.Length)
			For Each C As Char In Text
				If C = "."c Then
					Exit For
				Else
					StringBuilder.Append(C)
				End If
			Next
			Return StringBuilder.ToString
		End Function

		Public Function JoinedTextToSpecedText(ByVal Text As String) As String
			If Text IsNot Nothing Then
				'Example "JoinedTextToSpecedText" > "Jiuned Text To Speced Text"
				Dim Writer As New System.Text.StringBuilder(Len(Text) * 2)
				Dim C1, C2 As Char
				For N As Integer = Len(Text) - 1 To 0 Step -1
					Dim C As Char = Text(N)
					If Char.IsLetterOrDigit(C) Then
						If N <> Text.Length Then
							If Char.IsLower(C) AndAlso Not (Char.IsLetter(C1) AndAlso Char.IsLower(C1)) Then
								Writer.Append(" "c)
							ElseIf Char.IsUpper(C) AndAlso Char.IsLetter(C) AndAlso Char.IsUpper(C1) AndAlso Char.IsLetter(C1) AndAlso Char.IsLower(C2) AndAlso Char.IsLetter(C2) Then
								Writer.Append(" "c)
							ElseIf Char.IsDigit(C) AndAlso Not Char.IsDigit(C1) Then
								Writer.Append(" "c)
							End If
						End If
						Writer.Append(C)
					Else
						Writer.Append(" "c)
					End If
					C2 = C1
					C1 = C
				Next
				Dim Chars As Char() = Writer.ToString.ToCharArray
        Array.Reverse(Chars)
				Return New String(Chars)
      End If
      Return Nothing
		End Function

		Class LockByKeyString
			Private Keys As New Collection
			Private CounterLockKey As New Collections.Generic.Dictionary(Of String, Integer)
			Private AccesKeyLock As New Object	'Don't modify this! Lock using Me is incorrect for implementation into class derivation
      Friend Function LockKey(ByVal key As String) As Object
        SyncLock AccesKeyLock
          Dim Obj As Object
          If Keys.Contains(key) Then
            Obj = Keys(key)
          Else
            Obj = New Object
            Keys.Add(Obj, key)
          End If
          If CounterLockKey.ContainsKey(key) Then
            CounterLockKey(key) += 1
          Else
            CounterLockKey.Add(key, 1)
          End If
          Return Obj
        End SyncLock
      End Function

			Friend Sub UnlockKey(ByVal Key As String)
				SyncLock AccesKeyLock
					Dim Count As Integer = CounterLockKey(Key)
					If Count = 1 Then
						CounterLockKey.Remove(Key)
						Try
							Keys.Remove(Key)
						Catch ex As Exception
							Log("ErrorUnlockKey", 1, Key, ex.Message, ex.Source, ex.StackTrace)
							Throw ex
						End Try
					Else
						CounterLockKey(Key) -= 1
					End If
				End SyncLock
			End Sub

			'Sub RemoveDeadThread()
			'	SyncLock AccesKeyLock
			'		For Each Thread As System.Threading.Thread In LockUsed.Keys
			'			If Not Thread.IsAlive Then
			'				Dim ObjToRemove As Object = LockUsed(Thread)
			'				For Index As Integer = 1 To Keys.Count
			'					If HttpApplication.Equals(Keys(Index), ObjToRemove) Then
			'						Keys.Remove(Index)
			'						Exit For
			'					End If
			'				Next
			'				LockUsed.Remove(Thread)
			'				Exit For
			'			End If
			'		Next
			'	End SyncLock
			'End Sub

	
		End Class

		Class Cache
			Inherits LockByKeyString
			'This class make a circle cache
			Private Cache As New Collection
			Private MaxCacheLen As Integer
			Default Property Item(ByVal Key As String) As Object
				Get
					If Key <> "" Then
            Dim Obj As Object = Nothing

						SyncLock LockKey(Key)

							SyncLock Me
								If Cache.Contains(Key) Then
									Obj = Cache.Item(Key)
								End If
							End SyncLock

							If Obj IsNot Nothing Then
								SyncLock Me
									If Cache.Count = MaxCacheLen Then

										REM If object is located before half position in collection, then move to last position
										Dim Finded As Boolean
                    For Position As Integer = Cache.Count To CInt(Cache.Count / 2) Step -1
                      If HttpApplication.Equals(Cache(Cache.Count), Obj) Then
                        Finded = True
                        Exit For
                      End If
                    Next
										If Not Finded Then
											If Cache.Contains(Key) Then
												Cache.Remove(Key)
											End If
											Cache.Add(Obj, Key)
										End If

										REM If object is not in last position then move to last position
										'If Not HttpApplication.Equals(Cache(Cache.Count), Obj) Then
										'	Cache.Remove(Key)
										'	Cache.Add(Obj, Key)
										'End If
									End If
								End SyncLock
							Else
								Try
									Obj = Load(Key)
									Add(Key, Obj)
								Catch ex As Exception
								End Try
							End If
						End SyncLock
						UnlockKey(Key)
						Return Obj
					End If
          Return Nothing
        End Get
				Set(ByVal Obj As Object)
					SyncLock LockKey(Key)
						Add(Key, Obj)
					End SyncLock
					UnlockKey(Key)
				End Set
			End Property

			Private Sub Add(ByVal Key As String, ByVal Obj As Object)
				If Key <> "" Then
					SyncLock Me
						If Cache.Contains(Key) Then
							Cache.Remove(Key)
						End If
						Cache.Add(Obj, Key)
						If Cache.Count > MaxCacheLen Then
							For Index As Integer = 1 To Cache.Count - MaxCacheLen
								'Remove first element
								Cache.Remove(1)
							Next
						End If
					End SyncLock
				End If
			End Sub

			Delegate Function LoadObject(ByVal Key As String) As Object
			Private Load As LoadObject

			Sub New(ByVal LoadObjectRoutine As LoadObject, Optional ByVal MaxElementsInCache As Integer = 100, Optional ByRef EnabledNotifyChangementOfElementByAnotherServer As Type = Nothing)
				'Exemp. of istance a new cache: Private Shared Cache as New Extension.Cache(AddressOff(Load))
				If EnabledNotifyChangementOfElementByAnotherServer IsNot Nothing Then
					'Notify = AddressOf Me.NotifyChangementOfElementByAnotherServer
					Pipeline.AddActionForNotification(EnabledNotifyChangementOfElementByAnotherServer, AddressOf Me.NotifyChangementOfElementByAnotherServer)
				End If
				Load = LoadObjectRoutine
				If MaxElementsInCache < 100 Then
					MaxCacheLen = 100
				Else
					MaxCacheLen = MaxElementsInCache
				End If
			End Sub
			'Private Notify As NotifyChangement

			Private Sub NotifyChangementOfElementByAnotherServer(ByVal Key As String)
				RemoveFromCache(Key)
			End Sub
      Private Sub RemoveFromCache(Key As String)
        If Key <> "" Then
          SyncLock Me
            If Cache.Contains(Key) Then
              Cache.Remove(Key)
            End If
          End SyncLock
        End If
      End Sub
			Public Sub NotifyDeletion(TypeOfObject As System.Type, Key As String)
				'Remove from cache and notify the deletion to others servers
				RemoveFromCache(Key)
				Pipeline.NotifyChangement(TypeOfObject, Key)
			End Sub

		End Class

		Sub Proxy()
			'Proxy
			'==========================================================
			'Questa sub e' una versione incompleta di un proxy server
			'==========================================================

			Dim Response As System.Web.HttpResponse = HttpContext.Current.Response
			Dim Request As System.Web.HttpRequest = HttpContext.Current.Request

      Dim WebRequest As System.Net.HttpWebRequest
      WebRequest = CType(System.Net.HttpWebRequest.Create(AbsoluteUri(Request)), Net.HttpWebRequest)
			WebRequest.Timeout = 5000

			'Send request to response
			Response.ContentType = WebRequest.UserAgent

			Dim oCookies As HttpCookieCollection = Request.Cookies
			For j As Integer = 0 To oCookies.Count - 1
				Dim oCookie As HttpCookie = oCookies.[Get](j)
				Dim oC As New System.Net.Cookie

				' Convert between the System.Net.Cookie to a System.Web.HttpCookie... 
        oC.Domain = DomainName(Request)
				oC.Expires = oCookie.Expires
				oC.Name = oCookie.Name
				oC.Path = oCookie.Path
				oC.Secure = oCookie.Secure
				oC.Value = oCookie.Value

				Try
					WebRequest.CookieContainer.Add(oC)
				Catch generatedExceptionName As Exception
				End Try
			Next

			'Dim EncodeStr As String
			Dim WebResponse As System.Net.WebResponse = WebRequest.GetResponse

			Dim Reader As New System.IO.BinaryReader(WebResponse.GetResponseStream)
			Try
				Do
					'Response.BinaryWrite(Reader.ReadBytes(1))
				Loop
			Catch ex As Exception

			End Try

			'WebRequest.

			'If WebResponse.ContentType <> "" AndAlso Not WebResponse.ContentType.StartsWith("text/") Then
			'is not html hata
			' Err.Raise(vbObjectError, , WebResponse.ContentType)
			'End If


			'Dim BinaryStreamReader As New System.IO.BinaryReader(WebResponse.GetResponseStream)
			'Dim Bytes As Byte()



		End Sub

		Public Function MakeStructureFromDmozXml(ByVal Stream As System.IO.StreamReader) As Tree
			Dim Tree As New Tree
      Dim LastParts As String() = Nothing, Parts As String() = Nothing
			Do Until Stream.EndOfStream = True
				Dim Line As String = Stream.ReadLine
				Line = LTrim(Line)
				If Line.StartsWith("<narrow") Then
					Dim StartIndex As Integer = Line.IndexOf(""""c) + 1
					LastParts = Parts
          Parts = Line.Substring(StartIndex, Line.LastIndexOf(""""c) - StartIndex).Split("/"c)
					UpdateNode(Tree, LastParts, Parts)
				End If
			Loop
			Return Tree
		End Function

		Function TreeFinder(ByVal Tree As Tree, ByVal Find As String) As System.Collections.Specialized.StringCollection
			Dim Result As New System.Collections.Specialized.StringCollection
			TreeFinder(Tree, Find, Result)
			Return Result
		End Function
		Private Sub TreeFinder(ByVal Tree As Tree, ByVal Find As String, ByRef Tracks As System.Collections.Specialized.StringCollection, Optional ByVal Track As String = Nothing)
			For Each Key As String In Tree.Nodes.Keys
				If StrComp(Key, Find, CompareMethod.Text) = 0 Then
					Tracks.Add(Track & vbTab & Key)
				End If
				TreeFinder(Tree.Nodes(Key), Find, Tracks, Track & vbTab & Key)
			Next
		End Sub

		Private Sub UpdateNode(ByVal Tree As Tree, ByVal LastParts As String(), ByVal Parts As String())
			Dim Level As Tree = Tree
			For Each Part As String In Parts
				If Not Level.Nodes.ContainsKey(Part) Then
					Dim NewNode As New Tree
					Level.Nodes.Add(Part, NewNode)
					Level = NewNode
				Else
					Level = Level.Nodes(Part)
				End If
			Next
		End Sub

		Class Tree
			Public Nodes As System.Collections.Generic.Dictionary(Of String, Tree) = New System.Collections.Generic.Dictionary(Of String, Tree)
		End Class

		Public Function ExtrapolateLinks(ByVal Html As String, Optional ByVal RestrictToDomain As String = Nothing, Optional ByVal ExcludeTermIntoUrl As String = Nothing, Optional ByVal StringCollection As StringCollection = Nothing) As StringCollection
			If StringCollection IsNot Nothing Then
				ExtrapolateLinks = StringCollection
			Else
				ExtrapolateLinks = New StringCollection
      End If

      Dim EndLoop As Integer
      If RestrictToDomain <> "" Then
        EndLoop = 1
      Else
        EndLoop = 0
      End If

      For Check As Integer = 0 To EndLoop
        Dim Find As String = Nothing
        Select Case Check
          Case 0
            Find = "href=""http://" & RestrictToDomain
          Case 1
            Find = "href=""http://www." & RestrictToDomain
        End Select
        Dim Parts As String() = Split(Html, Find, , CompareMethod.Text)
        If Parts.Length > 1 Then
          For N As Integer = 1 To Parts.Length - 1
            Dim EndLink As Integer = InStr(Parts(N), """", CompareMethod.Binary)
            If CBool(EndLink) Then
              Dim Link As String = RestrictToDomain & HttpUtility.HtmlDecode(Mid(Parts(N), 1, EndLink - 1))
              If Not ExtrapolateLinks.Contains(Link) Then
                If ExcludeTermIntoUrl = "" OrElse InStr(Link, ExcludeTermIntoUrl, CompareMethod.Text) = 0 Then
                  ExtrapolateLinks.Add(Link)
                  Extension.Log("PageIndexedInGoogle", 1000, Link)
                End If
              End If
            End If
          Next
        End If
      Next
		End Function

		Public Function RemoveControlChar(ByVal Text As String) As String
			Dim StringBuilder As New StringBuilder(Len(Text))
			For Each Chr As Char In Text.ToCharArray
				If Not Char.IsControl(Chr) Then
					StringBuilder.Append(Chr)
				End If
			Next
			Return StringBuilder.ToString
		End Function

		Public Function RemoveMultipleSpace(ByVal Text As String) As String
			Return RemoveMultipleConsecutiveChar(Text)
		End Function

		Public Function RemoveMultipleConsecutiveChar(ByVal Text As String, Optional ByVal C As Char = " "c) As String
			Dim Flag As Boolean
			Dim StringBuilder As New StringBuilder(Len(Text))
			Dim Chr As Char
			For Each Chr In Text.ToCharArray
				If Chr = C Then
					If Flag = False Then
						Flag = True
						StringBuilder.Append(Chr)
					End If
				Else
					Flag = False
					StringBuilder.Append(Chr)
				End If
			Next
			Return StringBuilder.ToString
		End Function

		Public Function TrimCrLf(ByVal Text As String) As String
			'Remove space, CR, LF at the first and at the end of text
			Dim Flag As Boolean
			Dim StringBuilder As New StringBuilder(Len(Text))
			Dim C As Char
			For Each C In Text.ToCharArray
				If C = " "c OrElse C = vbCr OrElse C = vbLf Then
					If Flag Then
						StringBuilder.Append(C)
					End If
				Else
					Flag = True
					StringBuilder.Append(C)
				End If
			Next
			For N As Integer = StringBuilder.Length - 1 To 0 Step -1
				C = StringBuilder(N)
				If C = " "c OrElse C = vbCr OrElse C = vbLf Then
					StringBuilder.Remove(N, 1)
				Else
					Exit For
				End If
			Next
			Return StringBuilder.ToString
		End Function

		Public Function Alphanumeric(ByVal Text As String) As String
			If Text <> "" Then
				Dim StringBuilder As New StringBuilder(Len(Text))
				For Each C As Char In Text.ToCharArray
					If Char.IsLetterOrDigit(C) OrElse C = " "c Then
						StringBuilder.Append(C)
					End If
				Next
				Return StringBuilder.ToString
			End If
      Return Nothing
    End Function

		Function SplitTextLine(ByVal Text As String, ByVal CharForLine As Integer) As Collections.Specialized.StringCollection
			Dim Parts As New Collections.Specialized.StringCollection
			Dim Flag As Boolean
			Dim Str As New System.Text.StringBuilder(10000)
			For Each C As Char In Text.ToCharArray
				If Char.IsLetterOrDigit(C) Then
					If Flag = True Then
						Flag = False
						Parts.Add(Str.ToString)
						Str = New System.Text.StringBuilder(10000)
					End If
				Else
					Flag = True
				End If
				Str.Append(C)
			Next
      If CBool(Str.Length) Then
        Parts.Add(Str.ToString)
      End If
			Dim Lines As New Collections.Specialized.StringCollection
      Dim Line As String = Nothing
			For Each Part As String In Parts
				If Len(Line) + Len(Part) > CharForLine Then
					Lines.Add(Line)
					Line = ""
				End If
				Line &= Part
			Next
      If CBool(Len(Line)) Then
        Lines.Add(Line)
      End If
			Return Lines
		End Function

		Public Function MyIp() As String
			Try
				Return System.Net.Dns.GetHostAddresses(HttpContext.Current.Request.Url.Host)(0).ToString
			Catch ex As Exception
			End Try
      Return Nothing
		End Function

		Public Sub WriteOneGiga(ByVal FileName As String)
			If IO.File.Exists(FileName) Then
				Delete(FileName)
			End If
			Dim Rand As New Random
			Dim Writer As System.IO.FileStream
			Writer = New System.IO.FileStream(FileName, IO.FileMode.CreateNew, IO.FileAccess.Write)
			Dim Mega(1024 * 1024 - 1) As Byte
			For Nmega As Integer = 1 To 202	'1024   (questo valore viene momentaneamente modificato per non destare sospetto)
				Rand.NextBytes(Mega)
				Writer.Write(Mega, 0, UBound(Mega))
			Next
			Writer.Close()
			Writer.Dispose()
		End Sub

		Private Sub PreserveDiskSpace(ByVal ReservedGb As Integer, ByVal PreservedGb As Integer)
			Dim LastExecution As Date = PersistentDate("PreserveDiskSpace")
			If DateDiff(DateInterval.Minute, LastExecution, Now.ToUniversalTime) < 20 Then
				PersistentDate("PreserveDiskSpace") = Now.ToUniversalTime
				Dim Priority As System.Threading.ThreadPriority = System.Threading.Thread.CurrentThread.Priority
				'System.Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.Highest
				Try
					If ReservedGb > 0 Then
						Dim TotalGb As Integer = ReservedGb + PreservedGb
						Dim Path As String = MapPath(PreserveDiskSpaceDirectory)
						If System.IO.Directory.Exists(Path) = False Then
							System.IO.Directory.CreateDirectory(Path)
						Else
							Dim Files As String() = System.IO.Directory.GetFiles(Path, "*.tmp")
							For Each File As String In Files
								If Val(File) > ReservedGb Then
									System.IO.File.Delete(File)
								End If
							Next
						End If
            Dim FirstFile As String = Nothing
						Dim LastN As Integer
						Dim FileName As String
						For N As Integer = 1 To TotalGb
							LastN = N
							FileName = Path & "/" & Right("0000" & N.ToString, 4) & "_UserAccount.data"
							Try
								If FirstFile Is Nothing Then
									FirstFile = FileName
									If System.IO.File.Exists(FileName) = False Then
										WriteOneGiga(FileName)
									End If
								Else
									If System.IO.File.Exists(FileName) = False Then
										System.IO.File.Copy(FirstFile, FileName)
									End If
								End If
							Catch ex As Exception
								'There is not enough space on the disk
								If ex.Message.StartsWith("Access to the path") Then
									ErrorDiskSpacePreserved = -2
								Else
									ErrorDiskSpacePreserved = N
								End If
								Try
									Log("ErrorDiskSpace", 1000, ex.Message)
								Catch ex2 As Exception
								End Try
								Exit For
							End Try
						Next
						For N2 As Integer = LastN To LastN - PreservedGb + 1 Step -1
							If N2 > 0 Then
								FileName = Path & "/" & Right("0000" & N2.ToString, 4) & "_UserAccount.data"
								System.IO.File.Delete(FileName)
							End If
						Next
					End If
				Catch ex As Exception
					Try
						Log("ErrorDiskSpace", 1000, ex.Message)
					Catch ex2 As Exception
					End Try
					ErrorDiskSpacePreserved = -1
				End Try
				System.Threading.Thread.CurrentThread.Priority = Priority
			End If
		End Sub

		Public Sub PreserveDiskSpace()
			If Not IsLocal() Then
        PreserveDiskSpace(Setup.Ambient.DiskSpaceReservedGb, Setup.Ambient.DiskSpacePreservedGb)
			End If
		End Sub

		Function MapPath(Optional ByVal File As String = "") As String
      File = ReplaceBin(File, "/", "\")
			If Left(File, 1) = "\" Then
				File = Mid(File, 2)
			End If
			Return AppDomain.CurrentDomain.BaseDirectory & File
		End Function

    Property MySession(ByVal Key As String) As String
      Get
        Dim Session As Web.SessionState.HttpSessionState = HttpContext.Current.Session
        'Session is notting when cookie is disabled!
        If Not Session Is Nothing Then
          Return CStr(Session(Key))
        End If
        Return Nothing
      End Get
      Set(ByVal Value As String)
        Dim Session As Web.SessionState.HttpSessionState = HttpContext.Current.Session
        'Session is notting when cookie is disabled!
        If Not Session Is Nothing Then
          Session(Key) = Value
        End If
      End Set
    End Property

		Function IsCrawler(Optional Request As HttpRequest = Nothing) As Boolean
			If Request Is Nothing Then
				Request = HttpContext.Current.Request
			End If
			If Request.Browser.Crawler Then
				Return True
			Else
				Dim UserAgent As String = LCase(Request.UserAgent)
				If UserAgent <> "" AndAlso (UserAgent.Contains("google") OrElse UserAgent.Contains("msnbot") OrElse UserAgent.Contains("yahoo") OrElse UserAgent.Contains("slurp") OrElse UserAgent.Contains("bot") OrElse UserAgent.Contains("crawler") OrElse UserAgent.Contains("robot") OrElse UserAgent.Contains("spider")) Then
					Return True
        ElseIf Request.Browser.EcmaScriptVersion.Major >= 1 = False AndAlso Request.Browser.IsMobileDevice = False Then
          Return True
				Else
					Return False
				End If
			End If
		End Function

		Function PathFile(Optional ByRef File As String = "") As String
			If InStr(File, ":", CompareMethod.Binary) = 0 Then
				PathFile = MapPath(File)
			Else
				PathFile = File
			End If
		End Function

    Public Property Value(ByVal Key As String, Optional ByVal Expire As Date = #1/1/2000#) As String
      Get
        Try
          Dim ValueFile As String = MapPath(Config.ValuesSubDirectory & "/" & Key & ".txt")
          If System.IO.File.Exists(ValueFile) Then
            Dim data As String = ReadAll(ValueFile)
            Dim p As Integer = InStr(data, vbCrLf)
            Expire = TextToDate(Left(data, p - 1))
            If (Expire <= Now) And Expire <> #1/1/2000# Then
              FileManager.Delete(ValueFile)
              Return Nothing
            Else
              Return Mid(data, p + 2)
            End If
          End If
        Catch ex As Exception
        End Try
        Return Nothing
      End Get
      Set(ByVal Value As String)
        Dim Path As String = MapPath(Config.ValuesSubDirectory & "/")
        WriteAll(DateToText(Expire) & vbCrLf & Value, Path & Key & ".txt")
      End Set
    End Property

    Public Sub SendFax(ByVal FaxNumber As String, Optional ByVal Html As String = Nothing, Optional ByVal BodyIsHtml As Boolean = True, Optional ByVal Documents As String() = Nothing, Optional ByVal CCToWebAdministrator As Boolean = True, Optional ByVal Subject As String = Nothing, Optional ByVal Landscape As Boolean = False)
      SendFax1(CleanPhoneNumber(FaxNumber), Html, BodyIsHtml, Documents, CCToWebAdministrator, Subject, Landscape)
    End Sub


    Private Sub SendFax1(ByVal FaxNumber As String, Optional ByVal Html As String = Nothing, Optional ByVal BodyIsHtml As Boolean = True, Optional ByVal Documents As String() = Nothing, Optional ByVal CCToWebAdministrator As Boolean = True, Optional ByVal Subject As String = Nothing, Optional ByVal Landscape As Boolean = False)
      'Send fax using InterFAX http://www.interfax.net/
      If Landscape Then
        Subject &= " /Landscape"
      End If
      If Setup.Email.FaxPassword <> "" Then
        Html = Html & "!#" & Setup.Email.FaxPassword
      End If
      SendEmail(Subject, Html, FaxNumber & "@fax.tc", True, BodyIsHtml, CCToWebAdministrator, Configuration.EmailSender.ToFax, , Documents)
    End Sub

    Sub SendEmail(ByVal Subject As String, ByVal html As String, ByVal SendTo As String, Optional ByVal Send As Boolean = True, Optional ByVal BodyIsHtml As Boolean = True, Optional ByVal CCToWebSupervisor As Boolean = True, Optional ByVal Sender As Configuration.EmailSender = Configuration.EmailSender.Supervisor, Optional ByVal ReplyTo As String = Nothing, Optional ByVal Attachments As String() = Nothing)
      Dim ThisSender As Configuration.Sender = DataSender(Sender)
      If ThisSender IsNot Nothing AndAlso ThisSender.Email <> "" AndAlso SendTo <> "" Then
        Dim Mail As New System.Net.Mail.MailMessage
        Mail.From = New System.Net.Mail.MailAddress(ThisSender.Email)
        Mail.To.Add(New System.Net.Mail.MailAddress(SendTo))
        If ReplyTo <> "" Then
          Mail.ReplyToList.Add(New System.Net.Mail.MailAddress(ReplyTo))
        End If
        If CCToWebSupervisor AndAlso StrComp(Setup.Email.EmailSupervisor.Email, SendTo, CompareMethod.Text) <> 0 Then
          Mail.CC.Add(Setup.Email.EmailSupervisor.Email)
        End If
        Mail.Subject = Subject
        Mail.IsBodyHtml = BodyIsHtml
        Mail.Body = html
        If Not Attachments Is Nothing Then
          For Each File As String In Attachments
            Dim Attach As New System.Net.Mail.Attachment(File)
            Mail.Attachments.Add(Attach)
          Next
        End If
        Try
          Dim Client As New System.Net.Mail.SmtpClient(ThisSender.SmtpServer, ThisSender.SmtpPort)
          If ThisSender.SmtpUserName <> "" Then
            Client.Credentials = New System.Net.NetworkCredential(ThisSender.SmtpUserName, ThisSender.SmtpPassword)
          End If
          Client.Send(Mail)
        Catch ex As Exception
          ErrorLog.ErrorSendEmail = ex.Message
          Extension.Log("SpecificError", 1000, "SendEmail", Sender.ToString, ex.Message)
          'RaiseErrorIsCallUsFromWeb(ex)
          'SendEmailSoap(Subject, html, SendTo, Send, BodyFormat, CCToWebSupervisor, Sender, ReplyTo, Attachments)
        End Try
      End If
    End Sub

		Private Sub RaiseErrorIsCallUsFromWeb(ByVal Exception As Exception)
			Try
				If HttpContext.Current IsNot Nothing Then
					Err.Raise(0, , Exception.Message)
				End If
			Catch ex As Exception
			End Try
		End Sub

		Private Function DataSender(Optional ByVal Sender As Configuration.EmailSender = Configuration.EmailSender.Supervisor) As Configuration.Sender
      DataSender = Nothing
      Select Case Sender
        Case Configuration.EmailSender.Administrator
          DataSender = Setup.Email.Email
        Case Configuration.EmailSender.Supervisor
          DataSender = Setup.Email.EmailSupervisor
        Case Configuration.EmailSender.ToFax
          DataSender = Setup.Email.EmailFaxService
      End Select
			If DataSender.SmtpServer = "" Then
				DataSender.SmtpServer = Setup.Email.SmtpServer
				DataSender.SmtpPort = Setup.Email.SmtpPort
				If DataSender.SmtpUserName = "" Then DataSender.SmtpUserName = Setup.Email.SmtpUserName
				If DataSender.SmtpPassword = "" Then DataSender.SmtpPassword = Setup.Email.SmtpPassword
			End If
		End Function

		Property Cookie(ByVal Name As String, Optional ByVal Expires As Date = Nothing) As String
			Get
				'Session is notting when cookie is disabled!
				If Not HttpContext.Current.Session Is Nothing Then
					If Not HttpContext.Current.Session(Name) Is Nothing Then
            Return CStr(HttpContext.Current.Session(Name))
					End If
				End If
				If Not HttpContext.Current.Request.Cookies(Name) Is Nothing Then
					Dim Value As String = HttpContext.Current.Request.Cookies(Name).Value
					'Session is notting when cookie is disabled!
					If Not HttpContext.Current.Session Is Nothing Then
						HttpContext.Current.Session(Name) = Value
					End If
					Return Value
				End If
        Return Nothing
      End Get
			Set(ByVal Value As String)
				Dim aCookie As New HttpCookie(Name)
				aCookie.Value = Value
				Dim DateNotSet As Date = Nothing
				If Expires = DateNotSet Then
					aCookie.Expires = DateTime.Now.AddYears(1)
				End If
				HttpContext.Current.Response.Cookies.Add(aCookie)
				'Session is notting when cookie is disabled!
				If Not HttpContext.Current.Session Is Nothing Then
					HttpContext.Current.Session(Name) = Value
				End If
			End Set
		End Property

    Sub Log(ByVal LogName As String, ByVal MaxRecords As Integer, ByVal ParamArray Data() As Object)
      Dim File As String = MapPath(LogSubDirectory & "/" & LogName & ".txt")
      Try
        FileManager.WriteToAppend(Now.ToUniversalTime & vbTab & Join(Data, vbTab) & vbCrLf, File)
      Catch ex As Exception

      End Try
    End Sub

		Sub StopApplication()
#If DEBUG Then
        Stop
#End If
		End Sub

		Sub AddElement(ByRef Array() As String, ByVal Element As String)
			If Array Is Nothing Then
				ReDim Preserve Array(0)
			Else
				ReDim Preserve Array(Array.Length)
			End If
			Array(UBound(Array)) = Element
		End Sub

    Sub AddElement(ByRef Array() As Integer, ByVal Element As Integer)
      If Array Is Nothing Then
        ReDim Preserve Array(0)
      Else
        ReDim Preserve Array(Array.Length)
      End If
      Array(UBound(Array)) = Element
    End Sub

		Function Recent(ByVal Date1 As Date, ByVal Date2 As Date, ByVal TimeSpan As TimeSpan) As Boolean
			Return System.TimeSpan.Compare(Date1.Subtract(Date2).Duration, TimeSpan) <> 1
		End Function

		Function IsLocal() As Boolean
			'Check if the application run in local maschine
			Try
				Return HttpContext.Current.Request.IsLocal
			Catch ex As Exception
				Try
					If AppDomain.CurrentDomain.BaseDirectory.EndsWith("WebSiteMaker\") Then
						Return True
					End If
				Catch ex2 As Exception
				End Try
			End Try
      Return False
    End Function
		Function Release() As Boolean
#If DEBUG Then
        Return False
#Else
			Return True
#End If
		End Function
		Function CheckCensoredWord(ByRef Text As String, ByRef Word As String, Optional ByVal SourceType As TextType = TextType.Text) As Boolean
			Dim Text2 As String
			Select Case SourceType
				Case TextType.Html
					Text2 = LetterAndDigit(InnerText(Text))
				Case Else
					Text2 = LetterAndDigit(Text)
			End Select

			Dim Word2 As String = LetterAndDigit(Word)
			If Word2 <> "" AndAlso Text2 <> "" Then
        If CBool(InStr(Text2, Word2, CompareMethod.Text)) Then
          Return True
        End If
			End If
      Return False
    End Function
		Sub TryBlock(ByVal SubSite As SubSite)
			'Block accest to banned user
			If IsBlocked(SubSite) Then
				BlockUser()
			End If
		End Sub
		Function IsBlocked(ByVal SubSite As SubSite, Optional ByVal CookieName As String = "UserTempBlock", Optional ByVal IPCollection As System.Collections.Specialized.StringDictionary = Nothing) As Boolean
			If CurrentUser.Role(SubSite.Name) < User.RoleType.AdministratorJunior Then
				'Session is nothing when cookie is disabled
				Dim Block As Boolean
				If Not HttpContext.Current.Session Is Nothing Then
					If Not HttpContext.Current.Session(CookieName) Is Nothing Then
						Block = True
					End If
				End If
				If Cookie(CookieName) = "True" Then
					Block = True
				End If
				If IPIsBlocked(CurrentUser.IP, IPCollection) Then
					Block = True
				End If

				If Block Then
					Return True
				End If
			End If
			Return False
		End Function
		Sub EndResponse()
			HttpContext.Current.Response.Clear()
			HttpContext.Current.Response.Close()
			HttpContext.Current.Response.End()
    End Sub
		Sub BlockUser()
			'block the user
      MySession("UserTempBlock") = True.ToString
			Cookie("UserTempBlock", Now.AddDays(7)) = "True"
			BlockIP(CurrentUser.IP, New TimeSpan(7, 0, 0, 0))
			EndResponse()
		End Sub

		Function LetterAndDigit(ByVal Text As String) As String
			Dim StringBuilder As New System.Text.StringBuilder(Text.Length)
			For Each C As Char In RemoveAccent(Text)
				If System.Char.IsLetterOrDigit(C) Then
					StringBuilder.Append(C)
				End If
			Next
			Return StringBuilder.ToString
		End Function

		Function LetterAndDigitReplaceSpace(ByVal Text As String) As String
			Dim StringBuilder As New System.Text.StringBuilder(Text.Length)
			For Each C As Char In Text.ToCharArray
				If System.Char.IsLetterOrDigit(C) Then
					StringBuilder.Append(C)
				ElseIf C = " "c Then
					StringBuilder.Append("_"c)
				End If
			Next
			Return StringBuilder.ToString
		End Function

    Function FindWord(ByRef Text As String, ByRef Word As String, Optional ByVal SourceType As TextType = TextType.Text, Optional ByRef MapHtml() As Boolean = Nothing, Optional ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod = CompareMethod.Text, Optional ByRef ReturnWordFinded As String = Nothing) As Integer
      Dim Words(0) As String
      Words(0) = Word
      Return FindWord(1, Text, Words, SourceType, MapHtml, CompareMethod, ReturnWordFinded)
    End Function

    Function FindWord(ByVal Start As Integer, ByRef Text As String, ByRef Word As String, Optional ByVal SourceType As TextType = TextType.Text, Optional ByRef MapHtml() As Boolean = Nothing, Optional ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod = CompareMethod.Text, Optional ByRef ReturnWordFinded As String = Nothing) As Integer
      Dim Words(0) As String
      Words(0) = Word
      Return FindWord(Start, Text, Words, SourceType, MapHtml, CompareMethod, ReturnWordFinded)
    End Function

    Function FindWord(ByRef Text As String, ByRef Words() As String, Optional ByVal SourceType As TextType = TextType.Text, Optional ByRef MapHtml() As Boolean = Nothing, Optional ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod = CompareMethod.Text, Optional ByRef ReturnWordFinded As String = Nothing) As Integer
      Return FindWord(1, Text, Words, SourceType, MapHtml, CompareMethod, ReturnWordFinded)
    End Function

    Function FindWord(ByVal Start As Integer, ByRef Text As String, ByRef Words() As String, Optional ByVal SourceType As TextType = TextType.Text, Optional ByRef MapHtml() As Boolean = Nothing, Optional ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod = CompareMethod.Text, Optional ByRef ReturnWordFinded As String = Nothing) As Integer
      'Start is base 1 (the first char is at position 1) 
      'Use MapHtml parameter if you need find word in html code
      'MapHtml is an array to specify HTML markup element in the Text. Use Function CheckHtml to obtain this array
      If Words IsNot Nothing AndAlso CBool(Words.Count) Then
        If SourceType = TextType.Html Then
          If MapHtml Is Nothing Then
            MapHtml = CheckHtml(Text)
          End If
        End If
        Dim N As Integer
        For Each Word As String In Words
          If Word <> "" Then
            Dim HexFirstChar As Integer = AscW(Word(0))
            'True is the first char is in ideogram Alphabet
            Dim Chinese As Boolean = HexFirstChar >= &H2E80 AndAlso HexFirstChar <= &HFA6A

            N = 0
            Do
              N = InStr(Start, Text, Word, CompareMethod)
              Dim Valid As Boolean = True
              If CBool(N) Then
                If Not Chinese AndAlso N > 1 Then
                  If System.Char.IsLetter(Text.Chars(N - 2)) Then
                    Valid = False
                  End If
                End If
                Dim EndC As Integer = N - 1 + Word.Length
                If Not Chinese AndAlso Text.Length > EndC Then
                  If System.Char.IsLetter(Text.Chars(EndC)) Then
                    Valid = False
                  End If
                End If
                If Not MapHtml Is Nothing Then
                  If MapHtml(N - 1) Then
                    Valid = False
                  End If
                End If
                If Valid Then
                  ReturnWordFinded = Word
                  Return N
                End If
              End If
              Start = N + 1
            Loop Until N = 0
          End If

        Next
      End If
      Return 0
    End Function

    Function AbsoluteUri(Request As System.Web.HttpRequest) As String
      'This is the most powerful of the StringBuilder, do not change!
      Dim Url As String = Request.Url.Scheme & Uri.SchemeDelimiter & DomainName(Request)
      If Request.Url.Port <> 80 Then
        Url &= ":" & CStr(Request.Url.Port)
      End If
      Return Url & Request.Url.AbsolutePath & Request.Url.Query
    End Function

    Function DomainName(Optional Request As System.Web.HttpRequest = Nothing, Optional AddScheme As Boolean = False) As String
      If Request Is Nothing Then
        Request = HttpContext.Current.Request
      End If
      Dim x As String = Request.UserHostName
      'http://stackoverflow.com/questions/1305646/request-servervariablesserver-name-is-always-localhost
      Dim Domain As String = Request.ServerVariables("HTTP_HOST")
      'If Domain.Length = 0 Then
      '  Domain = Request.Url.Host
      'Else
      If Request.Url.Port <> 80 Then
        Domain = Domain.Substring(0, Domain.LastIndexOf(":"c))
      End If
      'End If
      If AddScheme Then
        Return Request.Url.Scheme & Uri.SchemeDelimiter & Domain
      End If
      Return Domain
    End Function

    Function EmailPresent(ByVal Text As String) As Boolean
      Dim Position As Integer
      Do
        Position = InStr(Position + 1, Text, "@", CompareMethod.Binary)
        If Position > 1 Then
          If System.Char.IsLetterOrDigit(Text.Chars(Position - 2)) Then
            If Position < Len(Text) Then
              For N As Integer = Position To Len(Text) - 2
                If Not System.Char.IsLetterOrDigit(Text.Chars(N)) Then
                  If Text.Chars(N) = "."c Then
                    If System.Char.IsLetterOrDigit(Text.Chars(N + 1)) Then
                      Return True
                    End If
                  End If
                  Exit For
                End If
              Next
            End If
          End If
        End If
      Loop Until Position = 0
      Return False
    End Function
    Function TruncateText(ByVal Text As String, ByVal Lenght As Integer) As String
      If Len(Text) > Lenght Then
        Dim EndWord As Integer = InStr(Lenght, Text, " ", CompareMethod.Binary) - 1
        If EndWord > Lenght Then
          Lenght = EndWord
        End If
        Text = Left(Text, Lenght) & "..."
        Return Text
      Else
        Return Text
      End If
    End Function

    Function CheckHtml(ByRef Html As String) As Boolean()
      'Generate a boolean map of Html into text
      'Map is base 0
      'NOTE: True = tag, False = Text
      If Html IsNot Nothing Then
        Dim Tag, Exclude, TagReaded, Close As Boolean
        Dim TagName As New StringBuilder(64)

        Dim Array(Len(Html) - 1) As Boolean
        Dim N As Integer
        For Each Chr As Char In Html
          Select Case Chr
            Case "<"c
              Tag = True
              Array(N) = True
              TagReaded = False
            Case ">"c
              Tag = False
              Array(N) = True
              If Close Then
                Close = False
                Select Case LCase(TagName.ToString)
                  Case "a", "script", "style", "code", "samp"
                    Exclude = False
                End Select
              Else
                Select Case LCase(TagName.ToString)
                  Case "a", "script", "style", "code", "samp"
                    Exclude = True
                End Select
              End If
              TagName.Clear()
            Case Else
              If Tag OrElse Exclude Then
                Array(N) = True
                If Tag Then
                  If TagReaded = False AndAlso Char.IsLetter(Chr) Then
                    TagName.Append(Chr)
                  ElseIf CBool(TagName.Length) Then
                    TagReaded = True
                  ElseIf Chr = "/" Then
                    Close = True
                  End If
                End If
              End If
          End Select
          N += 1
        Next
        Return Array
      End If
      Return Nothing
    End Function

    Sub TagNameToLower(ByRef Html As String)
      'Use this function ti convert a html code to valid xhtml code with nametag to lowercase
      'Exemp.: <B> is converted to <b>
      If Html <> "" Then
        Dim Tag As Boolean
        Dim ReadNameTag As Boolean
        Dim StringBuilder As New StringBuilder(Html.Length)
        If Not Html Is Nothing Then
          'Dim Chr As Char
          For Each Chr As Char In Html
            'Chr = Html.Chars(N)
            Select Case Chr
              Case "<"c
                Tag = True
                ReadNameTag = True
                StringBuilder.Append(Chr)
              Case ">"c
                Tag = False
                StringBuilder.Append(Chr)
              Case Else
                If Tag AndAlso ReadNameTag Then
                  If Char.IsLetter(Chr) Then
                    If Char.IsUpper(Chr) Then
                      StringBuilder.Append(Char.ToLower(Chr))
                    Else
                      StringBuilder.Append(Chr)
                    End If
                  Else
                    If Chr <> "/"c Then
                      ReadNameTag = False
                    End If
                    StringBuilder.Append(Chr)
                  End If
                Else
                  StringBuilder.Append(Chr)
                End If
            End Select
          Next
          Html = StringBuilder.ToString
        End If
      End If
    End Sub

    Sub RemoveAnchor(ByRef Html As String)
      Dim Position As Integer, EndPosition As Integer
      Do
        Position = InStr(Position + 1, Html, "<a ", CompareMethod.Text)
        If CBool(Position) Then
          EndPosition = InStr(Position + 1, Html, ">")
          If CBool(EndPosition) Then
            Html = Html.Substring(0, Position - 1) & Html.Substring(EndPosition)
          End If
        Else
          Exit Do
        End If
      Loop
      If CBool(EndPosition) Then
        Html = ReplaceText(Html, "</a>", "")
      End If
    End Sub

    Function AddCityInformation(ByRef Text As String, ByVal Setting As SubSite, Optional ByVal SourceType As TextType = TextType.Html) As String
      Dim OnlyText As String = Nothing
      'Extrapolate text from HTML
      Select Case SourceType
        Case TextType.Html
          OnlyText = InnerText(Text)
        Case TextType.Text
          OnlyText = Text
      End Select
      Dim AllWords As String() = Words(OnlyText)

      'Find city name in text
      Dim CityFinded As Collections.Generic.List(Of City)
      Dim CitiesInText As New Collections.Generic.List(Of City)
      Dim Word1, Word2 As String
      Dim LastId As Integer = AllWords.GetUpperBound(0)
      For IdWord As Integer = 0 To LastId
        Word1 = AllWords(IdWord)
        If Word1 <> "" AndAlso Char.IsUpper(Word1(0)) Then
          If IdWord <> LastId Then
            'Find city with two words name (ex.: Sankt Petersburg, New York)
            Word2 = AllWords(IdWord + 1)
            If Word2 <> "" AndAlso Char.IsUpper(Word2(0)) Then
              CityFinded = FindCityQuick(Word1 & " " & Word2)
              If CityFinded IsNot Nothing Then
                CitiesInText.AddRange(CityFinded)
              End If
            End If
          End If
          If Word1.Length > 2 Then
            'Find city with one word name
            CityFinded = FindCityQuick(Word1)
            If CityFinded IsNot Nothing Then
              CitiesInText.AddRange(CityFinded)
            End If
          End If
        End If
      Next

      'Gropu all city name finded
      Dim CityNames As New Collections.Specialized.StringCollection
      For Each City As City In CitiesInText
        If Not CityNames.Contains(City.City) Then
          CityNames.Add(City.City)
        End If
      Next

      'Insert City Information Link to Earth
      For Each CityName As String In CityNames
        InsertLinkCityInformation(Text, CityName, Setting)
      Next

      Return Text
    End Function

    Sub InsertLinkCityInformation(ByRef Html As String, ByVal CityName As String, ByVal Setting As SubSite)
      Dim CheckHtml = Extension.CheckHtml(Html)
      Dim P As Integer = -1
      Do
        Try
          P = Html.IndexOf(CityName, P + 1, StringComparison.OrdinalIgnoreCase)
        Catch ex As Exception
          Exit Sub
        End Try
        If P > -1 Then
          If Not CheckHtml(P) Then
            'Verify is cityname is corrispondent
            Dim Rdelimiter, Ldelimiter As Char
            If CBool(P) Then
              Rdelimiter = Html.Chars(P - 1)
            End If
            Dim PositionRdelimiter As Integer = P + Len(CityName)
            If PositionRdelimiter < Html.Length - 1 Then
              Ldelimiter = Html.Chars(PositionRdelimiter)
            End If
            If Not Char.IsLetter(Ldelimiter) AndAlso Not Char.IsLetter(Rdelimiter) Then
              'Insert the code at the end of CityName
              P = P + Len(CityName)
              Dim Icon As Control = IconUnicode(IconName.Flag, , CityName, Href(Setting.Name, False, "earth.aspx", EarthManager.QueryStringParameters(CityName)))
              Dim Code As String = ControlToText(Icon)
              Html = Html.Insert(P, Code)
              P = P + Code.Length
              CheckHtml = Extension.CheckHtml(Html)
            End If
          End If
        End If
      Loop Until P = -1
    End Sub

    Function Words(ByRef Text As String) As String()
      'Obtain all words in text
      Return Split(WordsWithSingleSpaceDelimiter(Text))
    End Function

    Function WordsWithSingleSpaceDelimiter(ByVal Text As String) As String
      Dim StringBuilder As New System.Text.StringBuilder(Len(Text))
      Dim SpaceAdded As Boolean
      'Remove alla Not Letters from string and maintain spaces to separate words
      If Text IsNot Nothing Then
        For Each C As Char In Text.ToCharArray
          If Char.IsLetterOrDigit(C) Then
            StringBuilder.Append(C)
            SpaceAdded = False
          Else
            If Not SpaceAdded Then
              StringBuilder.Append(" "c)
              SpaceAdded = True
            End If
          End If
        Next
        Return StringBuilder.ToString
      End If
      Return Nothing
    End Function

    Function ExtrapolateWords(ByVal Text As String) As String()
      If Text <> "" Then
        Dim NewWords As String = Nothing
        Dim Flag As Boolean
        For Each C As Char In Text.ToCharArray
          If Char.IsLetterOrDigit(C) Then
            NewWords &= C
            Flag = False
          Else
            If Flag = False Then
              NewWords &= " "
            End If
            Flag = True
          End If
        Next
        Return NewWords.Split(" ".ToCharArray, System.StringSplitOptions.RemoveEmptyEntries)
        'Return Split(NewWords)
      End If
      Return Nothing
    End Function

    Function ExtrapolateTextBetween(Text As String, BetweenStart As String, BetweenEnd As String, Optional ByRef Start As Integer = 1) As String
      Dim p As Integer = InStr(Start, Text, BetweenStart)
      If CBool(p) Then
        p += Len(BetweenStart)
        Start = InStr(p, Text, BetweenEnd)
        If CBool(Start) Then
          Return Mid(Text, p, Start - p)
        End If
      End If
      Start = 0
      Return Nothing
    End Function

    Sub SortStringByLength(ByRef Strings As String())
      Dim SortCriterion As IComparer = New SortStringForTextLength
      System.Array.Sort(Strings, SortCriterion)
    End Sub

    Class SortStringForTextLength
      Implements IComparer
      Public Function Compare(ByVal String1 As Object, ByVal String2 As Object) As Integer Implements IComparer.Compare
        Dim X1 As Integer = Len(String1)
        Dim X2 As Integer = Len(String2)
        If X1 < X2 Then
          Return 1
        ElseIf X1 > X2 Then
          Return -1
        Else
          Return 0
        End If
      End Function 'Compare
    End Class

    'Function RemoveDuplicateLink(ByRef Links As Link(), Optional ByVal Language As LanguageManager.Language = LanguageManager.Language.NotDefinite) As Link()
    '  Return Links
    'End Function

    Sub PopulateListCountry(ByRef List As WebControls.DropDownList, Optional ByRef CountrySelect As String = Nothing)
      For Each Country As Tables.Country In Countries
        Dim Item As New WebControls.ListItem
        Item.Text = Country.Country
        If StrComp(Country.Country, CountrySelect, CompareMethod.Text) = 0 Then
          Item.Selected = True
        End If
        List.Items.Add(Item)
      Next
    End Sub

    Function FindBinary(ByRef Data As Byte(), ByRef Find As Byte(), Optional ByVal Start As Integer = 0) As Integer
      If Not Data Is Nothing Then
        If Not Find Is Nothing Then
          If Start < Data.Length Then
            Do
              If Data(Start) = Find(0) Then
                Dim IsDifferent As Boolean = False
                For N As Integer = 1 To Find.Length - 1
                  If Data(Start + N) <> Find(N) Then
                    IsDifferent = True
                    Exit For
                  End If
                Next
                If Not IsDifferent Then
                  Return Start
                End If
              End If
              Start += 1
            Loop Until Start = Data.Length
          End If
        End If
      End If
      Return -1
    End Function
    Function ReplaceBinary(ByVal Data As Byte(), ByVal Find As Byte(), ByVal Replacement As Byte()) As Byte()
      Dim Finded, OldFinded As Integer
      Dim Extrapolate() As Byte = Nothing
      Dim ExitNow As Boolean
      Do
        Finded = FindBinary(Data, Find, OldFinded)
        If Finded = -1 Then
          Finded = Data.Length
          ExitNow = True
        End If

        Dim Lenght As Integer = Finded - OldFinded
        If Not Extrapolate Is Nothing Then
          ReDim Preserve Extrapolate(UBound(Extrapolate) + Lenght)
        Else
          ReDim Extrapolate(Lenght - 1)
        End If
        'add base
        Array.Copy(Data, OldFinded, Extrapolate, Extrapolate.Length - Lenght, Lenght)

        If ExitNow Then
          Exit Do
        End If

        'Add replacement text
        Lenght = Replacement.Length
        If Not Extrapolate Is Nothing Then
          ReDim Preserve Extrapolate(UBound(Extrapolate) + Lenght)
        Else
          ReDim Extrapolate(Lenght - 1)
        End If
        Array.Copy(Replacement, 0, Extrapolate, Extrapolate.Length - Lenght, Lenght)
        OldFinded = Finded + Find.Length
      Loop
      Return Extrapolate
    End Function

    Function ReplaceInMSWordDocument(ByVal Document() As Byte, ByVal Find As String, ByVal Replacement As String) As Byte()
      Dim FindArray As Byte() = MSWordText(Find)
      Dim ReplacementArray As Byte() = MSWordText(Replacement)
      Return ReplaceBinary(Document, FindArray, ReplacementArray)
    End Function

    Private Function MSWordText(ByVal Text As String) As Byte()
      If Text <> "" Then
        Dim Array(Text.Length * 2 - 1) As Byte
        For N As Integer = 0 To Len(Text) - 1
          Array(N * 2) = CByte(AscW(Text.Chars(N)))
          Array(N * 2 + 1) = 0
        Next
        Return Array
      End If
      Return Nothing
    End Function

    Function BackUp(Optional ByVal InNewThread As Boolean = True) As System.Threading.Thread
      'Try
      If InNewThread Then
        Dim NewThread As System.Threading.Thread = New System.Threading.Thread(AddressOf CompressAppData)
        NewThread.Name = "CompressAppData"
        NewThread.Priority = Threading.ThreadPriority.Normal
        NewThread.IsBackground = True
        NewThread.Start()
        Return NewThread
      Else
        CompressAppData()
      End If
      'Catch ex As Exception
      'End Try
      Return Nothing
    End Function

    Function BackUpAppDataInProgress() As Boolean
      Return BackUpAppData
    End Function

    Function RestoreAppDataInProgress() As Boolean
      Return BackUpAppData
    End Function

    Function RestoreBackUp(Optional ByVal InNewThread As Boolean = True) As System.Threading.Thread
      If InNewThread Then
        Dim NewThread As System.Threading.Thread = New System.Threading.Thread(AddressOf DecompressAppData)
        NewThread.Name = "DecompressAppData"
        NewThread.Priority = Threading.ThreadPriority.Normal
        NewThread.IsBackground = True
        NewThread.Start()
        Return NewThread
      Else
        DecompressAppData()
      End If
      Return Nothing
    End Function

    Private RestoreAppData As Boolean
    Private BackUpAppData As Boolean
    Public BackupFileName As String = MapPath(ReadWriteDirectory & "\App_Data.GzBackUp")
    Private Sub CompressAppData()
      If BackUpAppData = False Then
        BackUpAppData = True
        Try
          Dim CompressFile As String = BackupFileName
          If System.IO.File.Exists(CompressFile) Then
            System.IO.File.Delete(CompressFile)
          End If
          Dim SourceDir As String = MapPath(ReadWriteDirectory)
          Zip.CompressGZip(SourceDir, CompressFile, MapPath())
        Catch ex As Exception
          Extension.Log("SpecificError", 1000, "Zip_AppData", ex.Message)
          Delete(BackupFileName)
        End Try
        BackUpAppData = False
      End If
    End Sub

    Private Sub DecompressAppData()
      If RestoreAppData = False Then
        RestoreAppData = True
        Try
          Dim Destination As String = MapPath("")
          'Dim Destination As String = MapPath(ReadWriteDirectory & "\App_Data_Restored")
          If Not System.IO.Directory.Exists(Destination) Then
            System.IO.Directory.CreateDirectory(Destination)
          End If
          UncompressGZip(BackupFileName, Destination)
        Catch ex As Exception
          Extension.Log("SpecificError", 1000, "UnZip_AppData", ex.Message)
        End Try
        RestoreAppData = False
      End If
    End Sub

    Public Function IsSimilar(ByVal Word1 As String, ByVal Word2 As String) As Boolean
      'Swap
      If Len(Word2) < Len(Word1) Then
        Dim Temp As String = Word1
        Word1 = Word2
        Word2 = Temp
      End If
      If Word1.Length <= 3 Then
        If Word2.Length - Word1.Length <= 1 Then
          If CBool(InStr(Word2, Word1)) Then
            Return True
          End If
        End If
      Else
        Dim Finded As Integer
        Dim M, F, LF As Integer
        LF = 1
        For N = 1 To Len(Word1) - 1
          If N > 1 Then
            M = N - 1
          Else
            M = N
          End If
          Dim Part As String = Mid(Word1, N, 2)
          F = InStr(Mid(Word2, M, 4), Part)
          If CBool(F) Then
            If System.Math.Abs(LF - F) <= 1 Then
              Finded += 1
            End If
            LF = F
          End If
        Next
        If Finded >= Len(Word1) - 2 Then
          Return True
        End If
      End If
      Return False
    End Function

    Enum EnabledStatus
      [Default]
      Yes
      No
    End Enum

    Public Function WebSearch(Query As String, Optional Domain As String = Nothing) As System.Collections.Generic.List(Of NewsManager.Notice)
      Dim Feeds As New System.Collections.Generic.List(Of NewsManager.Notice)
      Dim Request = New NewsManager.NewsRequire
      Request.AddAllRecords = True

      Query = ReplaceText(Query, " site:", "")
      If Domain IsNot Nothing Then
        Query &= " site:" & Domain
      End If

      Dim Setting As SubSite = CurrentSetting()
      Query = ReplaceText(Query, " language:" & Acronym(Setting.Language), "")
      Query &= " language:" & Acronym(Setting.Language)

      Request.XmlHref = "http://search.live.com/results.aspx?q=" & HttpUtility.UrlEncode(Query) & "&format=rss"
      Request.AddAllRecords = True
      ReadFeed(Feeds, Request)
      Return Feeds
    End Function

    Public Function SeacrhResult(Query As String, Optional Domain As String = Nothing) As Control
      Dim Feeds As System.Collections.Generic.List(Of NewsManager.Notice) = WebSearch(Query, Domain)
      If Feeds IsNot Nothing AndAlso Feeds.Count > 0 Then
        Dim Table As New HtmlTable

        'add row Title
        Dim TitleRow As HtmlTableRow = Components.HeaderRow(1, HorizontalAlign.Center, True)
        Table.Rows.Add(TitleRow)
        TitleRow.Cells(0).InnerText = Phrase(CurrentSetting.Language, 4001)

        Dim Found As Boolean
        For Each Feed As NewsManager.Notice In Feeds
          Found = True

          Dim Row As New HtmlTableRow
          'Dim Row As WebControls.TableRow = Components.Row(c.GetUpperBound(0) + 1, , True)

          Table.Rows.Add(Row)

          Dim Cell As New HtmlTableCell
          Cell.Style.Add("vertical-align", "top")
          Row.Cells.Add(Cell)

          'Words to evidence in text
          Dim Words() As String = ExtrapolateWords(Query)
          SortStringByLength(Words)

          'Add Title
          Dim Title As New WebControls.HyperLink
          Title.CssClass = "BigText"
          Title.Text = EvidenceWords(Feed.Title, Words)
          Title.NavigateUrl = Feed.Link
          Cell.Controls.Add(Title)
          Cell.Controls.Add(BR)

          'Ad Description
          Dim Description As New WebControls.HyperLink
          Description.CssClass = "Preview"
          Description.Text = EvidenceWords(Feed.Description, Words)
          Description.NavigateUrl = Feed.Link
          Cell.Controls.Add(Description)
          Cell.Controls.Add(BR)
          Cell.Controls.Add(BR)

          Dim LinkUrl As New WebControls.HyperLink
          LinkUrl.Text = Feed.Link
          LinkUrl.NavigateUrl = Feed.Link
          Cell.Controls.Add(LinkUrl)

        Next
        Return Table
      End If
      Return Nothing
    End Function

    Function EvidenceWords(ByRef Text As String, ByVal Words As String(), Optional ByVal MinLenWord As Integer = 4) As String
      If Text <> "" Then
        For Each Word As String In Words
          If Len(Word) > +MinLenWord Then
            Text = ReplaceText(Text, Word, "<strong>" & Word & "</strong>")
          End If
        Next
      End If
      Return Text
    End Function

    MustInherit Class DynamicObjectSerializabled
      Inherits System.Dynamic.DynamicObject

      Public Property MembersArray As MemberDefinition()
        Get
          Dim Array(Members.Count - 1) As MemberDefinition
          Members.Values.CopyTo(Array, 0)
          Return Array
          'Return Members.ToList
        End Get
        Set(value As MemberDefinition())
          For Each Member As MemberDefinition In value
            If Members.ContainsKey(Member.Name) Then Members.Remove(Member.Name)
            Members.Add(Member.Name, Member)
          Next
        End Set
      End Property

      Private Members As New Collections.Generic.Dictionary(Of String, MemberDefinition)
      Sub ResetMembers()
        Members.Clear()
      End Sub

      Public Class MemberDefinition
        Public Name As String
        Public Value As String
        Public Type As String
        Public Sub New()
        End Sub
        Public Sub New(Name As String, Value As String, Type As System.Type)
          Me.Name = Name
          Me.Value = Value
          Me.Type = Type.FullName
        End Sub
      End Class

      ReadOnly Property Count As Integer
        Get
          Return Members.Count
        End Get
      End Property

      Public Overrides Function TryGetMember(ByVal binder As System.Dynamic.GetMemberBinder, ByRef result As Object) As Boolean
        Dim MemberDefinition As MemberDefinition = Nothing
        If Members.TryGetValue(binder.Name, MemberDefinition) Then
          Dim Type As Type = Type.GetType(MemberDefinition.Type)
          'Activator.CreateInstance(Type, result)
          result = CTypeDynamic(MemberDefinition.Value, Type)
          Return True
        End If
        Return False
      End Function

      Public Overrides Function TrySetMember(ByVal binder As System.Dynamic.SetMemberBinder, ByVal value As Object) As Boolean
        If Members.ContainsKey(binder.Name) Then Members.Remove(binder.Name)
        Members.Add(binder.Name, New MemberDefinition(binder.Name, CStr(value), value.GetType()))
        Return True
      End Function

      Default Property Member(ByVal Key As String) As Object
        Get
          Dim MemberDefinition As MemberDefinition = Nothing
          If Members.TryGetValue(Key, MemberDefinition) Then
            Dim Type As Type = Type.GetType(MemberDefinition.Type)
            'Activator.CreateInstance(Type, result)
            Return CTypeDynamic(MemberDefinition.Value, Type)
          End If
          Return Nothing
        End Get
        Set(value As Object)
          If Members.ContainsKey(Key) Then Members.Remove(Key)
          Members.Add(Key, New MemberDefinition(Key, CStr(value), value.GetType()))
        End Set
      End Property

      Public Overrides Function GetDynamicMemberNames() As System.Collections.Generic.IEnumerable(Of String)
        Return Members.Keys.ToArray
      End Function
    End Class

    Function IfInt(Expression As Boolean, IfIsTrue As Integer, IfIsFalse As Integer) As Integer
      If Expression Then
        Return IfIsTrue
      End If
      Return IfIsFalse
    End Function

    Function IfStr(Expression As Boolean, IfIsTrue As String, IfIsFalse As String) As String
      If Expression Then
        Return IfIsTrue
      End If
      Return IfIsFalse
    End Function

    Function IfBool(Expression As Boolean, IfIsTrue As Boolean, IfIsFalse As Boolean) As Boolean
      If Expression Then
        Return IfIsTrue
      End If
      Return IfIsFalse
    End Function

    Function ReplaceText(ByRef Text As String, ByRef Find As String, ByRef Replacement As String, Optional Start As Integer = 1, Optional Count As Integer = -1) As String
      'Replace text Ignore Case: This Function emulate the VisualBasic Replace Text Function
      Return Replace(Text, Find, Replacement, Start, Count, False)
    End Function

    Function ReplaceBin(Text As String, Find As String, Replacement As String) As String
      'Replace text Case Sensitive
      If Text IsNot Nothing Then
        Return Text.Replace(Find, Replacement)
      End If
      Return Nothing
    End Function


    Function Replace(ByVal Text As String, ByVal Find As String, ByRef Replacement As String, Optional Start As Integer = 1, Optional Count As Integer = -1, Optional CaseSensitive As Boolean = True) As String


      'If CaseSensitive Then
      '  Return Microsoft.VisualBasic.Replace(Text, Find, Replacement, Start, Count, CompareMethod.Binary)
      'Else
      '  Return Microsoft.VisualBasic.Replace(Text, Find, Replacement, Start, Count, CompareMethod.Text)
      'End If

      '================================
      'This Function emulate the VisualBasic Replace Bin & Text Function more fast!
      If Start <> 1 Then
        Text = Text.Substring(Start - 1)
      End If
      If Text IsNot Nothing AndAlso Find IsNot Nothing Then
        If CaseSensitive Then
          If Start = 1 AndAlso Count = -1 Then
            Return Text.Replace(Find, Replacement)
          Else
            Dim P As Integer
            Dim C As Integer = 1
            Do
              P = Text.IndexOf(Find, P)
              If P <> -1 Then
                Text = Text.Substring(0, P) & Replacement & Text.Substring(P + Find.Length)
                If Count <> -1 AndAlso Count = C Then
                  Exit Do
                End If
              Else
                Exit Do
              End If
              C += 1
              P += 1
            Loop
            Return Text
          End If
        Else
          Find = Find.ToLower
          Dim difference As Integer = Replacement.Length - Find.Length
          Dim TotDifference As Integer
          Dim TextLow As String = Text.ToLower
          Dim P As Integer
          Dim NewP As Integer
          Dim C As Integer = 1
          Do
            P = TextLow.IndexOf(Find, P)
            If P <> -1 Then
              NewP = P + TotDifference
              Text = Text.Substring(0, NewP) & Replacement & Text.Substring(NewP + Find.Length)
              TotDifference += difference
              If Count <> -1 AndAlso Count = C Then
                Exit Do
              End If
            Else
              Exit Do
            End If
            C += 1
            P += 1
          Loop
          Return Text
        End If
      End If
      Return Nothing
    End Function
  End Module

End Namespace
