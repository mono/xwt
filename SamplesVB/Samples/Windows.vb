Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class Windows
        Inherits VBox

        Public Sub New()
            Dim b As Button = New Button("Show borderless window")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim w As Window = New Window()
                                      w.Decorated = False
                                      Dim c As Button = New Button("This is a window")
                                      w.Content = c
                                      AddHandler c.Clicked, Sub()
                                                                w.Dispose()
                                                            End Sub
                                      Dim bpos As Rectangle = b.ScreenBounds
                                      w.ScreenBounds = New Rectangle(bpos.X, bpos.Y + b.Size.Height, w.Width, w.Height)
                                      w.Show()
                                  End Sub
            b = New Button("Show message dialog")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      MessageDialog.ShowMessage(MyBase.ParentWindow, "Hi there!")
                                  End Sub
            Dim db As Button = New Button("Show custom dialog")
            MyBase.PackStart(db)
            AddHandler db.Clicked, Sub()
                                       Dim d As Dialog = New Dialog()
                                       d.Title = "This is a dialog"
                                       Dim t As Table = New Table()
                                       t.Attach(New Label("Some field:"), 0, 1, 0, 1)
                                       t.Attach(New TextEntry(), 1, 2, 0, 1)
                                       t.Attach(New Label("Another field:"), 0, 1, 1, 2)
                                       t.Attach(New TextEntry(), 1, 2, 1, 2)
                                       d.Content = t
                                       Dim custom As Command = New Command("Custom")
                                       d.Buttons.Add(New DialogButton(custom))
                                       d.Buttons.Add(New DialogButton("Custom OK", Command.Ok))
                                       d.Buttons.Add(New DialogButton(Command.Cancel))
                                       d.Buttons.Add(New DialogButton(Command.Ok))
                                       Dim r As Command = d.Run(Me.ParentWindow)
                                       db.Label = "Result: " + r.Label
                                       d.Dispose()
                                   End Sub
            b = New Button("Show Open File dialog")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim dlg As OpenFileDialog = New OpenFileDialog("Select a file")
                                      dlg.InitialFileName = "Some file"
                                      dlg.Multiselect = True
                                      dlg.Filters.Add(New FileDialogFilter("Xwt files", {"*.xwt"}))
                                      dlg.Filters.Add(New FileDialogFilter("All files", {"*.*"}))
                                      If dlg.Run() Then
                                          MessageDialog.ShowMessage("Files have been selected!", String.Join(vbLf, dlg.FileNames))
                                      End If
                                  End Sub
            b = New Button("Show Save File dialog")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim dlg As SaveFileDialog = New SaveFileDialog("Select a file")
                                      dlg.InitialFileName = "Some file"
                                      dlg.Multiselect = True
                                      dlg.Filters.Add(New FileDialogFilter("Xwt files", {"*.xwt"}))
                                      dlg.Filters.Add(New FileDialogFilter("All files", {"*.*"}))
                                      If dlg.Run() Then
                                          MessageDialog.ShowMessage("Files have been selected!", String.Join(vbLf, dlg.FileNames))
                                      End If
                                  End Sub
            b = New Button("Show Select Folder dialog (Multi select)")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim dlg As SelectFolderDialog = New SelectFolderDialog("Select some folder")
                                      dlg.Multiselect = True
                                      If dlg.Run() Then
                                          MessageDialog.ShowMessage("Folders have been selected!", String.Join(vbLf, dlg.Folders))
                                      End If
                                  End Sub
            b = New Button("Show Select Folder dialog (Single select)")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim dlg As SelectFolderDialog = New SelectFolderDialog("Select a folder")
                                      dlg.Multiselect = False
                                      If dlg.Run() Then
                                          MessageDialog.ShowMessage("Folders have been selected!", String.Join(vbLf, dlg.Folders))
                                      End If
                                  End Sub
            b = New Button("Show Select Color dialog")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim dlg As SelectColorDialog = New SelectColorDialog("Select a color")
                                      dlg.SupportsAlpha = True
                                      dlg.Color = Colors.AliceBlue
                                      If dlg.Run(MyBase.ParentWindow) Then
                                          MessageDialog.ShowMessage("A color has been selected!", dlg.Color.ToString())
                                      End If
                                  End Sub
            b = New Button("Show window shown event")
            MyBase.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim w As Window = New Window()
                                      w.Decorated = False
                                      Dim c As Button = New Button("This is a window with events on")
                                      w.Content = c
                                      AddHandler c.Clicked, Sub()
                                                                w.Dispose()
                                                            End Sub

                                      AddHandler w.Shown, Sub(sender As Object, args As EventArgs)
                                                              MessageDialog.ShowMessage("My Parent has been shown")
                                                          End Sub
                                      AddHandler w.Hidden, Sub(sender As Object, args As EventArgs)
                                                               MessageDialog.ShowMessage("My Parent has been hidden")
                                                           End Sub
                                      w.Show()
                                  End Sub
        End Sub
    End Class
End Namespace
