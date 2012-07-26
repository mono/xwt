Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class ButtonSample
        Inherits VBox

        Public Sub New()
            Dim b1 As Button = New Button("Click me")
            AddHandler b1.Clicked, Sub()
                                       b1.Label = "Clicked!"
                                   End Sub
            MyBase.PackStart(b1)
            Dim b2 As Button = New Button("Click me")
            b2.Style = ButtonStyle.Flat
            AddHandler b2.Clicked, Sub()
                                       b2.Label = "Clicked!"
                                   End Sub
            MyBase.PackStart(b2)
            MyBase.PackStart(New Button(Image.FromIcon("ZoomIn", IconSize.Medium)))
            Dim mb As MenuButton = New MenuButton("This is a Menu Button")
            Dim men As Menu = New Menu()
            men.Items.Add(New MenuItem("First"))
            men.Items.Add(New MenuItem("Second"))
            men.Items.Add(New MenuItem("Third"))
            men.Items.Add(New SeparatorMenuItem())
            men.Items.Add(New CheckBoxMenuItem("Check") With {.Checked = True})
            men.Items.Add(New RadioButtonMenuItem("Radio") With {.Checked = True})
            men.Items.Add(New MenuItem("With image") With {.Image = Image.FromResource(GetType(App), "class.png")})
            mb.Menu = men
            MyBase.PackStart(mb)
            For Each mi As MenuItem In men.Items
                Dim cmi As MenuItem = mi
                AddHandler mi.Clicked, Sub()
                                           mb.Label = cmi.Label + " Clicked"
                                       End Sub
            Next
            Dim tb As ToggleButton = New ToggleButton("Toggle me")
            MyBase.PackStart(tb)
            MyBase.PackStart(New Button("Mini button") With {.Style = ButtonStyle.Borderless})
            MyBase.PackStart(New ToggleButton("Mini toggle") With {.Style = ButtonStyle.Borderless})
        End Sub
    End Class
End Namespace
