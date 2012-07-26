Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class ListBoxSample
        Inherits VBox

        Private name As DataField(Of String) = New DataField(Of String)()

        Private icon As DataField(Of Image) = New DataField(Of Image)()

        Public Sub New()
            Dim list As ListBox = New ListBox()
            For i As Integer = 0 To 100 - 1
                list.Items.Add("Value " + i)
            Next
            MyBase.PackStart(list, BoxMode.FillAndExpand)
            Dim customList As ListBox = New ListBox()
            Dim store As ListStore = New ListStore(Me.name, Me.icon)
            customList.DataSource = store
            customList.Views.Add(New ImageCellView(Me.icon))
            customList.Views.Add(New TextCellView(Me.name))

            Dim png As Image = Image.FromResource(GetType(App), "class.png")
            For i As Integer = 0 To 100 - 1
                Dim r As Integer = store.AddRow()
                store.SetValue(Of Image)(r, Me.icon, png)
                store.SetValue(Of String)(r, Me.name, "Value " + i)
            Next
            MyBase.PackStart(customList, BoxMode.FillAndExpand)
        End Sub
    End Class
End Namespace
