﻿' This is simply an example application of how to use the UOLite DLL.
' This demonstrates very BASIC usage principals.

'Imports UOLite2

Public Class frmMain
    'Declare a new instance of the "LiteClient" class named "Client"
    'The content folder should be your UO 2D Client directory.
    Public WithEvents Client As New UOLite2.LiteClient("C:\Program Files (x86)\EA Games\Ultima Online Mondain's Legacy")

    'Used to track the master of this bot in the game, via serial#
    Public Master As UOLite2.Serial = New UOLite2.Serial(0)

    'Used to track the bot's mount.
    Public Mount As UOLite2.Mobile = Nothing

    'The password for a normal user to say to gain exclusive control over the user.
    Public Password As String = "obey"

    'What happens when the login button is clicked.
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim ConnectResponse As String = Client.GetServerList(TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text)

        'ConnectedResponse is equal to "SUCCESS" if the CONNECTION was successful, authentication errors are handled by the "onLoginDenied" event.
        If ConnectResponse = "SUCCESS" Then

            'Logs the event in a textbox, using a delegate.
            Log("Connected to server: " & Client.LoginServerAddress & ":" & Client.LoginPort)

            'Changes the current tab in the tabview control.
            TabControl1.SelectTab(1)

            'Sets the "Send" button to be pressed when the user hits "enter" on the keyboard.
            Me.AcceptButton = SendButton

            'Moves the cursor to the command entry box.
            CmdBox.Select()
        Else
            'Logs a failure to connect and the reason for failure.
            Log(ConnectResponse)
        End If

    End Sub

    Private Sub Client_onLoginConfirm(ByRef Player As UOLite2.Mobile) Handles Client.onLoginConfirm
        'Client.Send("BF-00-0A-00-0F-0A-00-00-00-3F")
        'Client.Send("BF-00-09-00-0B-45-4E-55-00")
    End Sub

    Private Sub Client_onLoginDenied(ByRef Reason As String) Handles Client.onLoginDenied
        MsgBox(Reason)
    End Sub

    Private Sub Client_onCharacterListReceive(ByRef Client As UOLite2.LiteClient, ByVal CharacterList As System.Collections.ArrayList) Handles Client.onCharacterListReceive
        'Chooses the first character in the list.

        Client.ChooseCharacter(DirectCast(CharacterList.Item(0), UOLite2.Structures.CharListEntry).Name, DirectCast(CharacterList.Item(0), UOLite2.Structures.CharListEntry).Password, DirectCast(CharacterList.Item(0), UOLite2.Structures.CharListEntry).Slot)
    End Sub

    Private Sub Client_onCliLocSpeech(ByRef Client As UOLite2.LiteClient, ByVal Serial As UOLite2.Serial, ByVal BodyType As UShort, ByVal SpeechType As UOLite2.Enums.SpeechTypes, ByVal Hue As UShort, ByVal Font As UOLite2.Enums.Fonts, ByVal CliLocNumber As UInteger, ByVal Name As String, ByVal ArgsString As String) Handles Client.onCliLocSpeech
        'Logs the cliloc speech.
        Log("CliLoc: " & Name & " : " & Client.CliLocStrings.Entry(CliLocNumber))
    End Sub

    Private Sub Client_onError(ByRef Description As String) Handles Client.onError
        'Logs the error.
        Log(Description)
    End Sub

#Region "Scavenger Stuff"

    Private Sub AddScavengerTypes()
        'Add gold coins to the list of item types for the scavenger to look for.
        Client.Scavenger.AddType(UOLite2.Enums.Common.ItemTypes.GoldCoins)
    End Sub

#End Region

    Private Sub Client_onLoginComplete() Handles Client.onLoginComplete
        'Invokes a delegate to update the player position labels on the character tab.
        Me.Invoke(New _UpdatePlayerPosition(AddressOf UpdatePlayerPosition))

        'Logs the completion of login.
        Log("Login Complete")

        'Adds the types of things for the scavenger to look for.
        AddScavengerTypes()

        'Enables the scavenger.
        Client.Scavenger.Enabled = True
    End Sub

    'All of this code my be implemented in the next UOLite release.
#Region "Finding the player's mount"

    'A boolean used to tell the onNewMobile code that the next mobile that shows up should be marked as the player's mount.
    Private WaitingForMount As Boolean = False

    'When a mobile enters the screen this is called.
    Private Sub Client_onNewMobile(ByRef Client As UOLite2.LiteClient, ByVal Mobile As UOLite2.Mobile) Handles Client.onNewMobile
        If WaitingForMount = True Then
            'Sents the mount as a refernce to the mobile that just showed up.
            Mount = Mobile

            'Sets this to false so that way the mount isnt changed arbitrarily.
            WaitingForMount = False

            'Causes the client to say in-game, the mount's serial.
            Client.Speak("Mount Found: " & Mount.Serial.ToRazorString)

            'Checks to see if the player's mount is a beetle.
            If Mount.Type = 791 Then
                'Announce the setting change.
                Client.Speak("Setting beetle mount as scavenger storage container.")

                'Set the container for the scavenger to use, setting the alternate container also enables use of an alternate container.
                'If these next two lines where here it would place scavenged items in the players backpack.
                Client.Scavenger.AlternateContainer.SetContainer(Mount.Serial)

                'Let the scavenger know that it has to dismount your player to access the storage container.
                Client.Scavenger.AlternateContainer.DismountToAccess = True
            End If

            'Double clicks the mount to remount it.
            Mount.DoubleClick()
        End If
    End Sub

#End Region

    Private Sub Client_onSkillUpdate(ByRef Client As UOLite2.LiteClient, ByRef OldSkill As UOLite2.SupportClasses.Skill, ByRef NewSkill As UOLite2.SupportClasses.Skill) Handles Client.onSkillUpdate
        If NewSkill.BaseValue > OldSkill.BaseValue Then
            'Client.Speak(NewSkill.Name & " has increased by " & CDec((NewSkill.BaseValue - OldSkill.BaseValue) / 10) & "!")
        End If
    End Sub

    'Handles in-game speech.
    Private Sub Client_onSpeech(ByRef Client As UOLite2.LiteClient, ByVal Serial As UOLite2.Serial, ByVal BodyType As UShort, ByVal SpeechType As UOLite2.Enums.SpeechTypes, ByVal Hue As UShort, ByVal Font As UOLite2.Enums.Fonts, ByVal Text As String, ByVal Name As String) Handles Client.onSpeech
        'Debug.WriteLine(Text)
        Log("SPEECH: " & Name & " : " & Text)

        If Master.Value = 0 OrElse Master = Serial Then
            Select Case LCase(Text.Split(" ")(0))
                Case Password
                    Client.Speak(Name & ", you are now my master, and I shall obey your every command.")
                    Master = Serial

                Case "me"
                    Client.Speak(Name & ", your serial is " & BitConverter.ToString(Serial.GetBytes) & ", or " & Serial.Value)

                Case "you"
                    Client.Speak(Name & ", my serial is " & BitConverter.ToString(Client.Player.Serial.GetBytes) & ", or " & Client.Player.Serial.Value)

                Case "where"
                    Client.Speak(Name & ", you are at " & Client.Mobiles.Mobile(Serial).X & "," & Client.Mobiles.Mobile(Serial).Y & " which is " & Client.Get2DDistance(Client.Player, Client.Mobiles.Mobile(Serial)) & " paces to the " & Client.GetDirection(Client.Player, Client.Mobiles.Mobile(Serial)).ToString & " of me.")

                Case "say"
                    Client.Speak(Text.Substring(Text.Split(" ")(0).Length + 1))

                Case "walk"
                    Select Case Text.Split(" ").Length
                        Case 1
                            Client.Walk(Client.Player.Direction)

                        Case 2
                            Select Case LCase(Text.Split(" ")(1))
                                Case "north"
                                    Client.Walk(UOLite2.Enums.Direction.North)
                                Case "east"
                                    Client.Walk(UOLite2.Enums.Direction.East)
                                Case "south"
                                    Client.Walk(UOLite2.Enums.Direction.South)
                                Case "west"
                                    Client.Walk(UOLite2.Enums.Direction.West)
                                Case "up"
                                    Client.Walk(UOLite2.Enums.Direction.NorthWest)
                                Case "right"
                                    Client.Walk(UOLite2.Enums.Direction.NorthEast)
                                Case "down"
                                    Client.Walk(UOLite2.Enums.Direction.SouthEast)
                                Case "left"
                                    Client.Walk(UOLite2.Enums.Direction.SouthWest)
                            End Select

                        Case 3
                            Select Case LCase(Text.Split(" ")(1))
                                Case "north"
                                    Client.Walk(UOLite2.Enums.Direction.North, Text.Split(" ")(2))
                                Case "east"
                                    Client.Walk(UOLite2.Enums.Direction.East, Text.Split(" ")(2))
                                Case "south"
                                    Client.Walk(UOLite2.Enums.Direction.South, Text.Split(" ")(2))
                                Case "west"
                                    Client.Walk(UOLite2.Enums.Direction.West, Text.Split(" ")(2))
                                Case "up"
                                    Client.Walk(UOLite2.Enums.Direction.NorthWest, Text.Split(" ")(2))
                                Case "right"
                                    Client.Walk(UOLite2.Enums.Direction.NorthEast, Text.Split(" ")(2))
                                Case "down"
                                    Client.Walk(UOLite2.Enums.Direction.SouthEast, Text.Split(" ")(2))
                                Case "left"
                                    Client.Walk(UOLite2.Enums.Direction.SouthWest, Text.Split(" ")(2))
                            End Select


                    End Select
                Case "follow"
                    Client.Follow(Serial)

                Case "come"
                    Client.Walk(Client.GetDirection(Client.Player, Client.Mobiles.Mobile(Serial)))

                Case "stay"
                    Client.StopFollowing()

                Case "hide"
                    Client.Skills(UOLite2.Enums.Skills.Hiding).Use()

                Case "skill"
                    If Text.Split(" ").Length = 2 Then
                        Dim skillnum As UOLite2.Enums.Skills = Text.Split(" ")(1)
                        Client.Speak("Skill: " & Client.Skills(skillnum).Name)
                        Client.Speak("Value: " & Math.Round(CDec(Client.Skills(skillnum).Value / 10), 1))
                        Client.Speak("Base Value: " & Math.Round(CDec(Client.Skills(skillnum).BaseValue / 10), 1))
                        Client.Speak("Lock: " & Client.Skills(skillnum).Lock.ToString)
                        Client.Speak("Cap: " & Math.Round(CDec(Client.Skills(skillnum).Cap / 10), 1))
                    End If

                Case "ping"
                    Client.Speak("My packet round trip time is " & Client.Latency & "ms")

                Case "mcount"
                    Client.Speak("I know of " & Client.Mobiles.Count & " mobiles.")

                Case "icount"
                    Client.Speak("I know of " & Client.Items.Count & " items.")

                Case "weight"
                    Client.Speak(" My backpack weighs " & Client.Player.Weight & " stones, although I can carry " & Client.Player.MaxWeight & " stones.")

                Case "contents"
                    Client.Speak("I have " & Client.Player.Layers.BackPack.Contents.Count & " items in my backpack.")

                    For Each i As UOLite2.Item In Client.Player.Layers.BackPack.Contents.Items
                        Client.Speak("Item: " & i.Serial.ToRazorString & "," & i.Type & "," & i.Amount)
                    Next

                Case "dropall"

                    Client.Scavenger.Enabled = False

                    For Each i As UOLite2.Item In Client.Player.Layers.BackPack.Contents.Items
                        i.Drop()
                    Next


                Case "ts"
                    Client.Scavenger.Toggle(False)

                    Client.CastSpell(UOLite2.Enums.Spell.GreaterHeal)

                Case "findmount"
                    If Client.Player.IsMounted Then
                        'Let the rest of the code know to expect a mobile to show up, which is my mount.
                        WaitingForMount = True

                        'Doubleclick myself to dismount
                        Client.Player.DoubleClick()
                    End If

                Case "mount"
                    If Mount IsNot Nothing Then
                        Mount.DoubleClick()
                    End If

                Case "dmount", "dismount"
                    _Client.Player.DoubleClick()

                Case "sc"
                    Client.Speak("Container: " & Client.Scavenger.AlternateContainer.Container.ToRazorString)

            End Select
        End If

    End Sub

    Private Sub SendButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SendButton.Click
        Select Case LCase(CmdBox.Text.Split(" ")(0))
            Case "cliloccount"
                Log(Client.CliLocStrings.Count)
            Case "say"
                Client.Speak(CmdBox.Text.Substring(CmdBox.Text.Split(" ")(0).Length + 1), UOLite2.Enums.SpeechTypes.Regular)
                CmdBox.Clear()
            Case "send"
                Client.Send(CmdBox.Text.Substring(CmdBox.Text.Split(" ")(0).Length + 1))
            Case "allitems"
                Dim strbuild As New System.Text.StringBuilder

                For Each i As UOLite2.Item In Client.Items.Items
                    strbuild.AppendLine("-Item: " & i.Serial.ToRazorString)
                    strbuild.AppendLine(" Type: " & i.Type & " = " & i.TypeName)
                    strbuild.AppendLine(" Hue: " & i.Hue)
                    strbuild.AppendLine(" Stack: " & i.Amount)
                    strbuild.AppendLine(" X: " & i.X)
                    strbuild.AppendLine(" Y: " & i.Y)
                    strbuild.AppendLine(" Z: " & i.Z)
                    strbuild.AppendLine(" Prop Count: " & i.Properties.Count)

                    If i.Properties.Count > 0 Then
                        For x As UInt32 = 0 To i.Properties.Count - 1
                            strbuild.AppendLine(" Property: " & i.Properties(x).ToString)
                        Next
                    End If

                    strbuild.AppendLine(" PropStr: " & i.Properties.ToString)
                Next

                Log(strbuild.ToString)
            Case "test"
                Client.CastSpell(UOLite2.Enums.Spell.GreaterHeal)

        End Select
    End Sub

#Region "Logging Code"

    Public Delegate Sub ConsoleWrite(ByRef Text As String)

    Private Sub Client_onRecievedServerList() Handles Client.onRecievedServerList
        Client.ChooseServer(0)
    End Sub

    Private Sub ConsoleLog(ByRef Text As String)
        ConsoleBox.SuspendLayout()
        ConsoleBox.AppendText(Text)
        ConsoleBox.ResumeLayout()
    End Sub

    Private Sub Log(ByRef Text As String)
        Dim args(0) As Object
        args(0) = Text & vbNewLine
        Me.Invoke(New ConsoleWrite(AddressOf ConsoleLog), args)
    End Sub

#End Region

#Region "Movement"

    Private Sub Client_onPlayerMove(ByRef Client As UOLite2.LiteClient) Handles Client.onPlayerMove
        Me.Invoke(New _UpdatePlayerPosition(AddressOf UpdatePlayerPosition))
    End Sub

    Private Delegate Sub _UpdatePlayerPosition()

    Private Sub UpdatePlayerPosition()
        lbl_posx.Text = Client.Player.X
        lbl_posy.Text = Client.Player.Y
        lbl_posz.Text = Client.Player.Z
        lbl_Direction.Text = Client.Player.Direction.ToString
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        Client.Walk(UOLite2.Enums.Direction.North)
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        Client.Walk(UOLite2.Enums.Direction.South)
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Client.Walk(UOLite2.Enums.Direction.East)
    End Sub

    Private Sub Button10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button10.Click
        Client.Walk(UOLite2.Enums.Direction.West)
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Client.Walk(UOLite2.Enums.Direction.NorthWest)
    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        Client.Walk(UOLite2.Enums.Direction.SouthEast)
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Client.Walk(UOLite2.Enums.Direction.SouthWest)
    End Sub

    Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
        Client.Walk(UOLite2.Enums.Direction.NorthEast)
    End Sub

#End Region

    Private Sub Client_onTargetRequest(ByRef Client As UOLite2.LiteClient) Handles Client.onTargetRequest
        Log("Targets Requested")
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

    End Sub
End Class