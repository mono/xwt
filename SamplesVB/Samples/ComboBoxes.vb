Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class ComboBoxes
        Inherits VBox

        Public Sub New()
            Dim box As HBox = New HBox()
            Dim c As ComboBox = New ComboBox()
            c.Items.Add("One")
            c.Items.Add("Two")
            c.Items.Add("Three")
            c.SelectedIndex = 1
            box.PackStart(c)
            Dim la As Label = New Label()
            box.PackStart(la)
            AddHandler c.SelectionChanged, Sub()
                                               la.Text = "Selected: " + CStr(c.SelectedItem)
                                           End Sub
            MyBase.PackStart(box)
            box = New HBox()
            Dim c2 As ComboBox = New ComboBox()
            box.PackStart(c2)
            Dim b As Button = New Button("Fill combo (should grow)")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      For i As Integer = 0 To 10 - 1
                                          c2.Items.Add("Item " + New String("#", i))
                                      Next
                                  End Sub
            MyBase.PackStart(box)
            box = New HBox()
            Dim c3 As ComboBox = New ComboBox()
            c3.Items.Add(0, "Combo with custom labels")
            c3.Items.Add(1, "One")
            c3.Items.Add(2, "Two")
            c3.Items.Add(3, "Three")
            c3.Items.Add(ItemSeparator.Instance)
            c3.Items.Add(4, "Maybe more")
            Dim la3 As Label = New Label()
            box.PackStart(c3)
            box.PackStart(la3)
            AddHandler c3.SelectionChanged, Sub()
                                                la3.Text = "Selected item: " + c3.SelectedItem
                                            End Sub
            MyBase.PackStart(box)
            box = New HBox()
            Dim c4 As ComboBoxEntry = New ComboBoxEntry()
            Dim la4 As Label = New Label()
            box.PackStart(c4)
            box.PackStart(la4)
            c4.Items.Add(1, "One")
            c4.Items.Add(2, "Two")
            c4.Items.Add(3, "Three")
            c4.TextEntry.PlaceholderText = "This is an entry"
            AddHandler c4.TextEntry.Changed, Sub()
                                                 la4.Text = "Selected text: " + c4.TextEntry.Text
                                             End Sub
            MyBase.PackStart(box)
            Dim imgField As DataField(Of Image) = New DataField(Of Image)()
            Dim textField As DataField(Of String) = New DataField(Of String)()
            Dim descField As DataField(Of String) = New DataField(Of String)()
            Dim cbox As ComboBox = New ComboBox()
            Dim store As ListStore = New ListStore(textField, imgField, descField)
            cbox.ItemsSource = store
            Dim r As Integer = store.AddRow()
            store.SetValue(Of String)(r, textField, "Information")
            store.SetValue(Of String)(r, descField, "Icons are duplicated on purpose")
            store.SetValue(Of Image)(r, imgField, Image.FromIcon("Information", IconSize.Small))
            r = store.AddRow()
            store.SetValue(Of String)(r, textField, "Error")
            store.SetValue(Of String)(r, descField, "Another item")
            store.SetValue(Of Image)(r, imgField, Image.FromIcon("Error", IconSize.Small))
            r = store.AddRow()
            store.SetValue(Of String)(r, textField, "Warning")
            store.SetValue(Of String)(r, descField, "A third item")
            store.SetValue(Of Image)(r, imgField, Image.FromIcon("Warning", IconSize.Small))
            cbox.Views.Add(New ImageCellView(imgField))
            cbox.Views.Add(New TextCellView(textField))
            cbox.Views.Add(New ImageCellView(imgField))
            cbox.Views.Add(New TextCellView(descField))
            cbox.SelectedIndex = 0
            MyBase.PackStart(cbox)
        End Sub
    End Class
End Namespace
