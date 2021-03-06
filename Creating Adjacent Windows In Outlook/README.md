# Creating Adjacent Windows In Outlook
## Requires
- Visual Studio 2008
## License
- Apache License, Version 2.0
## Technologies
- Outlook
## Topics
- Outlook Customization
- Outlook UI
## Updated
- 12/06/2011
## Description

<div class="wikidoc">
<h1>Adding an adjacent window to the preview pane and inspectors in Outlook</h1>
<br>
<strong>Important:</strong> Windows API (HWND) integration in Outlook is not a supported platform technology.
<br>
<br>
Microsoft understands that customers are already using the Windows API approach to integrate with Outlook, and the Outlook Social Connector also leverages this type of integration. Documentation for Outlook adjacent windows provides &quot;best practice&quot; guidelines
 to help avoid conflicts with other programs using this approach, including the Outlook Social Connector.<br>
<br>
This sample code and the corresponding documentation are not supported by Microsoft. The Outlook product group does not consider this overall approach to be part of Outlook&rsquo;s supported architecture in terms of developing custom solutions with Outlook.
 Instead, Microsoft recommends and supports using other approaches that have been more fully designed, tested, and documented to work with Outlook. Depending on the version of Outlook, these other approaches include Outlook custom forms, form regions, folder
 home pages, custom task panes, and the Outlook Social Connector (OSC) extensibility architecture. One key advantage of using these supported approaches is that developers will have a much greater chance that they will not encounter compatibility issues when
 a newer version or service pack of Outlook is released. <br>
<br>
If you encounter issues using the information provided in the sample, you can post comments on the MSDN Code Gallery page for this download. However, Microsoft cannot guarantee that support will be provided for your issue.<br>
<h2>Summary</h2>
This article describes the method in which an add-in can add an adjacent window to the preview pane in Outlook. It is intended to work for Microsoft Office Outlook 2003, Microsoft Office Outlook 2007 and Microsoft Outlook 2010. After the add-in creates the
 window, it can operate and call any Windows APIs on the window exactly as any other HWND in Windows.
<br>
<h2>Definitions</h2>
<strong>Top level window</strong> &ndash; the Outlook HWND that is parented to the desktop &ndash; this is either the frame containing the Explorer or the Inspector.<br>
<strong>Sibling window</strong> &ndash; In the explorer, this is the preview pane. In the inspector, it is the window that contains the client area of the inspector (the mail header as well as the body). The adjacent window is placed adjacent to the sibling
 window.<br>
<strong>Adjacent window</strong> &ndash; The window that you (the add-in) are creating adjacent to the preview pane or inspector.<br>
<strong>Explorer</strong> &ndash; The main Outlook window that contains the Navigation Pane, Message List, and Preview Pane (among other things).<br>
<strong>Inspector</strong> &ndash; An Outlook window showing one specific item (mail, appointment, etc.).<br>
<strong>Legacy Outlook</strong> &ndash; Outlook 2003 and 2007. In this document, these versions are called legacy because they have a different HWND hierarchy than new Outlook (Outlook 2010).<br>
<h2>Prerequisites</h2>
Your add-in needs to have a mechanism to run tasks at idle. One way to do this is to create a hidden window that receives WM_TIMER messages. This works because WM_TIMER is a low priority message that runs when there are no higher priority messages to process.
 See idlecall.cpp.<br>
<h2>Finding all instances of Outlook</h2>
The <strong>FindTopLevelWindows</strong> function looks for all top level windows (with the Outlook message class &quot;rctrl_renwnd32&quot;). This function is first called when the add-in is initialized.<br>
<br>
To find Explorer frames, it traverses into the window hierarchy of the top level window to locate the view and preview pane (this is slightly different between Legacy Outlook and Outlook 2010). See
<strong>FindExplorerWindows</strong> and <strong>FindExplorerWindowsLegacyOutlook</strong>. In this case, if it does find the view and preview pane, the preview pane is considered the sibling window.<br>
<br>
If it doesn't find the preview pane, it then tries to find a window with the message class &quot;AfxWndW&quot;, which is the sibling window for an inspector.<br>
<br>
If the function finds the sibling window in either the Explorer orInspector case, it calls
<strong>FCreateHookedAdjacentWindow</strong> which creates the adjacent window. The parameters to
<strong>FCreateHookedAdjacentWindow</strong> are <strong>hWndSibling</strong>, the sibling window,
<strong>hWndParent</strong>, the parent to <strong>hWndSibling</strong> which should also be the parent of the adjacent window, and
<strong>hWndTopLevelWindow</strong>, the top level window. <strong>FCreateHookedAdjacentWindow</strong>, in addition to creating a window, also subclasses the sibling window with
<strong>WndProcHookedWindow</strong> which we will use to find out when the preview pane or inspector is resized, and the adjacent window needs to be adjusted as well.<br>
<br>
If we find an explorer window, but not a preview pane in the explorer, it means that the user has elected not to show the preview pane.
<strong>FindExplorerWindows</strong> and <strong>FindExplorerWindowsLegacyOutlook</strong> subclasses the parent window with
<strong>WndProcUnhookedWindow</strong> which listens for the WM_PARENTNOTIFY message which is sent when a child window is created. In this case, when we get this message, we call
<strong>FindTopLevelWindows</strong> again to see if the preview pane has been created.<br>
<br>
We also listen to the Outlook Object Model events <strong>Application.NewInspector</strong> and
<strong>Application.NewExplorer</strong> (see connect.cpp), and call the <strong>
FindTopLevelWindows</strong> function again when either message is received. Because Outlook sends these OM events before the corresponding explorer and inspector HWND is actually created, we need to wait till idle before calling
<strong>FindTopLevelWindows</strong> by using the idle function mechanism.<br>
<h2>Creating the adjacent window and associating the corresponding OM object with it</h2>
The <strong>FMatchWindowToOMObject</strong> function takes a top level window and tries to associate an OM object (either an Explorer or Inspector) to it. This is done by iterating through all explorers and inspectors, calling QueryInterface on the OM IDispatch
 object for the IOleWindow interface, calling IOleWindow::GetWindow, and then comparing the HWND returned by that function to the top level HWND. See
<strong>HwndFromIDispatch</strong>.<br>
<h2>Handling window subclassing</h2>
Both windows procedures we install, <strong>WndProcHookedWindow</strong> and <strong>
WndProcUnhookedWindow</strong>, must respond to the WM_REMOVING_WNDPROC message. Consider this scenario: Add-in A installs a window procedure by calling SetWindowLongPtr(GWLP_WNDPROC, saving off the previous wndproc). Add-in B installs a window procedure, saving
 off the previous wndproc (installed by Add-in A). Here is a schematic of what happens:<br>
<br>
<table border="1" cellspacing="0" cellpadding="0">
<tbody>
<tr>
<th>&nbsp;</th>
<th>Installed wndproc </th>
<th>Saved off old wndproc </th>
</tr>
<tr>
<td>Add-in A</td>
<td>WndProcA</td>
<td>WndProcO (the original)</td>
</tr>
<tr>
<td>Add-in B</td>
<td>WndProcB</td>
<td>WndProcA</td>
</tr>
</tbody>
</table>
<br>
In a typical situation where a window procedure is overridden, when Add-in A is unloaded, it would set the window procedure to WndProcO, and then when Add-in B unloads, it would set the window procedure to WndProcA. Both actions are wrong, and the second action
 would lead to a crash if the add-in containing WndProcA has been unloaded.<br>
<br>
To address this problem, each WndProc described in this document must support the WM_REMOVING_WNDPROC message. When an add-in is unloading, and wishes to restore the old WndProc, it first calls GetWindowLongPtr(GWLP_WNDPROC) and compares the current WndProc
 with the WndProc it installed. If they are equal, then it calls SetWindowLongPtr(GWLP_WNDPROC) with the saved old WndProc. If they are not equal, then it sends the WM_REMOVING_WNDPROC message to the HWND, with wParam == the old WndProc and lParam == the WndProc
 we installed. (See <strong>WndProcInfo::Destroy</strong>). The return value of sending the WM_REMOVING_WNDPROC message should always be 1 to signal that the message has been handled. Asserts are added in code to detect whether this procedure was done, for
 your debugging purposes (although there is nothing the add-in can do if the message was not handled).<br>
<br>
On receipt of the WM_REMOVING_WNDPROC message (see <strong>FHandleRemovingWndProc</strong>), if lParam == our old WndProc, then we swap our old WndProc with wParam. If this operation happens, then the WndProc returns the value 1. If the two values don't match,
 the message is forwarded to the old WndProc via CallWindowProc, as usual.<br>
<br>
For an example: if we look at the same situation as above with Add-in A and Add-in B, if Add-in A were to unload, it would send the WM_REMOVING_WNDPROC message to the window with wParam = WndProcO and lParam = WndProcA. Because WndProcB is the registered window
 handler, WndProcB would process the message first. Since lParam = WndProcA and the old WndProc stored by Add-in B is also WndProcB, then Add-in B would set its old WndProc to WndProcO, and return 1. Add-in A takes no additional action. After this, this is
 the schematic of Add-in B:<br>
<table border="1" cellspacing="0" cellpadding="0">
<tbody>
<tr>
<th>&nbsp;</th>
<th>Installed wndproc </th>
<th>Old wndproc </th>
</tr>
<tr>
<td>Add-in B</td>
<td>WndProcB</td>
<td>WndProcO</td>
</tr>
</tbody>
</table>
<br>
<h2>Overview of hosting multiple adjacent windows</h2>
When multiple add-ins wants to place adjacent windows to the preview pane, they will need to work cooperatively so that all the windows show up in an orderly fashion and do not overlap each other. To that end, the rest of this document describes how to cooperatively
 place windows in Explorers and Inspectors.<br>
<br>
We introduce the concept of the doubly linked list of adjacent windows. The linked list is not maintained by Outlook or by one single add-in &ndash; the 'next' and 'prev' links are maintained individually by each adjacent window. When an add-in wants to create
 an adjacent window, it adds itself to the head of the linked list (i.e. prev = NULL). The head of the linked list is the controller &ndash; this is the window that directs all the other windows in how to place themselves. The controller is also the one responsible
 for listening to the WM_WINDOWPOSCHANGING message of the sibling window (see <strong>
WndProcHookedWindow</strong>).<br>
<h2>Handling the WM_WINDOWPOSCHANGING message</h2>
We subclass the sibling window (<strong>WndProcHookedWindow</strong>) to handle the WM_WINDOWPOSCHANGING message. The purpose of handling this message for the sibling window is to adjust its location and size when it is being sized, so that room is made for
 the adjacent windows. When handling this message, the controller asks all of the adjacent windows (via the GetReservedRect message to be detailed later) how much room each window needs. It then subtracts that space from the sibling window's rect (via the WINDOWPOS
 structure from the WM_WINDOWPOSCHANGING message), and then tells all of the adjacent windows to move and layout themselves (via the PlaceWindowAdjacentToRect message). See the
<strong>CAdjacentWindow::SiblingWindowPosChanging</strong> function<br>
<h2>Handling the WM_NEGOTIATE_WINDOW_PANE message</h2>
All adjacent windows must process this message. For this message, the wParam parameter is set to a subcode that further specifies what the message is. See the
<strong>CAdjacentWindow::OnNegotiateWindow</strong> function. This message is sent by adjacent windows to communicate with each other. The next set of sections detail the different subcodes and their operation.<br>
<br>
<strong>AddCustomWindowToTop</strong><br>
This message is sent by a new adjacent window to add itself to the linked list of windows. lParam contains the HWND of the new adjacent window. See
<strong>CAdjacentWindow::FindTopMostInjectedPane</strong> for the process of sending this message. The function steps through all child windows of the parent of the sibling window, and sends this message to each window in turn until it receives a non-zero result.<br>
<br>
Non-controller windows respond to this message by returning 0. The controller responds to this message by setting its 'prev' window to lParam (thus making it not the controller), and returning its HWND, which the new adjacent window should use as the 'next'
 window (and the new window becomes the controller).<br>
<br>
If there are no existing controller windows, the new adjacent window becomes the controller.<br>
<br>
<strong>ReplacePrevWindow</strong><br>
This message is sent to replace the pointer to the 'prev' with the value of lParam. This is used by adjacent windows to maintain the linked list of windows when they are unloaded. (See
<strong>CAdjacentWindow::OnDestroy</strong>)<br>
<br>
<strong>ReplaceNextWindow</strong><br>
This message is sent to replace the pointer to the 'next' window with the value of lParam.<br>
<br>
<strong>GetReservedRect</strong><br>
This message is sent to a window to determine how much space to reserve for it. This message must be forwarded onto the 'next' window if present. lParam contains a RECT structure, which is initialized to 0,0,0,0 by the first caller. Each adjacent window adds
 to this structure how much space it needs. For example, an adjacent window to be placed below the people pane would add its desired height to the value of bottom. The values of left, top, right, and bottom are not coordinates, but are the total space desired
 (i.e. none of these values should be negative)<br>
<br>
<strong>PlaceWindowAdjacentToRect</strong><br>
This message is sent to an adjacent window when the adjacent window needs to position itself. lParam contains a RECT structure, which the original caller initializes to the client RECT of the sibling window. Each window places itself (possibly via SetWindowPos),
 then adjust the RECT structure so that the RECT now encompasses the adjacent window, then forwards the message onto the 'next' window. For example, an adjacent window to be placed below the preview pane with height CY would place itself at (rc.left, rc.bottom,
 rc.right &ndash; rc.left, CY), and then adjust the RECT structure to (rc.left, rc.top, rc.right, rc.bottom &#43; CY). When all adjacent windows have done their layout, the RECT structure should be identical to the client RECT of the parent window. The last assert
 in <strong>CAdjacentWindow::SiblingWindowPosChanging</strong> checks that this is the case.<br>
<br>
<strong>RecalcPaneLayout</strong><br>
This message is sent to ask the controller to do a full layout pass. Non-controller windows should forward this message up the linked list of adjacent windows. The controller (see CAdjacentWindow::RecalcPreviewPaneLayoutController) determines the current available
 space (the client area of the parent window), the required space (by sending a GetReservedRect message), then resizes the sibling window, and then sends the PlaceWindowAdjacentToRect message to have all the adjacent windows do their layout.</div>
