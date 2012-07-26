Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class MainWindow
        Inherits Window

        Private samplesTree As TreeView

        Private store As TreeStore

        Private icon As Image

        Private sampleBox As VBox

        Private title As Label

        Private currentSample As Widget

        Private nameCol As DataField(Of String) = New DataField(Of String)()

        Private widgetCol As DataField(Of Sample) = New DataField(Of Sample)()

        Private iconCol As DataField(Of Image) = New DataField(Of Image)()

        Private statusIcon As StatusIcon

        Public Sub New()
            Try
                Me.statusIcon = Application.CreateStatusIcon()
                Me.statusIcon.Menu = New Menu()
                Me.statusIcon.Menu.Items.Add(New MenuItem("Test"))
                Me.statusIcon.Image = Image.FromResource(MyBase.[GetType](), "package.png")
            Catch ex As Exception
                Console.WriteLine("Status icon could not be shown")
            End Try
            Dim menu As Menu = New Menu()
            Dim file As MenuItem = New MenuItem("File")
            file.SubMenu = New Menu()
            file.SubMenu.Items.Add(New MenuItem("Open"))
            file.SubMenu.Items.Add(New MenuItem("New"))
            Dim mi As MenuItem = New MenuItem("Close")
            AddHandler mi.Clicked, Sub()
                                       Application.[Exit]()
                                   End Sub
            file.SubMenu.Items.Add(mi)
            menu.Items.Add(file)
            Dim edit As MenuItem = New MenuItem("Edit")
            edit.SubMenu = New Menu()
            edit.SubMenu.Items.Add(New MenuItem("Copy"))
            edit.SubMenu.Items.Add(New MenuItem("Cut"))
            edit.SubMenu.Items.Add(New MenuItem("Paste"))
            menu.Items.Add(edit)
            MyBase.MainMenu = menu
            Dim box As HPaned = New HPaned()
            Me.icon = Image.FromResource(GetType(App), "class.png")
            Me.store = New TreeStore({Me.nameCol, Me.iconCol, Me.widgetCol})
            Me.samplesTree = New TreeView()
            Me.samplesTree.Columns.Add("Name", {Me.iconCol, Me.nameCol})
            Me.AddSample(Nothing, "Boxes", GetType(Boxes))
            Me.AddSample(Nothing, "Buttons", GetType(ButtonSample))
            Me.AddSample(Nothing, "CheckBox", GetType(Checkboxes))
            Me.AddSample(Nothing, "Clipboard", GetType(ClipboardSample))
            Me.AddSample(Nothing, "ColorSelector", GetType(ColorSelectorSample))
            Me.AddSample(Nothing, "ComboBox", GetType(ComboBoxes))
            Me.AddSample(Nothing, "Drag & Drop", GetType(DragDrop))
            Dim i As TreePosition = Me.AddSample(Nothing, "Drawing", Nothing)
            Me.AddSample(i, "Canvas with Widget", GetType(CanvasWithWidget))
            ' Me.AddSample(i, "Chart", GetType(ChartSample))
            Me.AddSample(i, "Colors", GetType(ColorsSample))
            Me.AddSample(i, "Figures", GetType(DrawingFigures))
            Me.AddSample(i, "Transformations", GetType(DrawingTransforms))
            Me.AddSample(i, "Images and Patterns", GetType(DrawingPatternsAndImages))
            Me.AddSample(i, "Text", GetType(DrawingText))
            Me.AddSample(i, "Partial Images", GetType(PartialImages))
            Me.AddSample(Nothing, "Progress bars", GetType(ProgressBarSample))
            Me.AddSample(Nothing, "Frames", GetType(Frames))
            Me.AddSample(Nothing, "Images", GetType(Images))
            Me.AddSample(Nothing, "Labels", GetType(Labels))
            Me.AddSample(Nothing, "ListBox", GetType(ListBoxSample))
            Me.AddSample(Nothing, "ListView", GetType(ListView1))
            Me.AddSample(Nothing, "Menu", GetType(MenuSamples))
            Me.AddSample(Nothing, "Notebook", GetType(NotebookSample))
            Me.AddSample(Nothing, "Paneds", GetType(PanedViews))
            Me.AddSample(Nothing, "Scroll View", GetType(ScrollWindowSample))
            Me.AddSample(Nothing, "Tables", GetType(Tables))
            Me.AddSample(Nothing, "Text Entry", GetType(TextEntries))
            Me.AddSample(Nothing, "Tooltips", GetType(Tooltips))
            Me.AddSample(Nothing, "TreeView", GetType(TreeViews))
            Me.AddSample(Nothing, "WidgetEvents", GetType(WidgetEvents))
            Me.AddSample(Nothing, "Windows", GetType(Windows))
            Me.samplesTree.DataSource = Me.store
            box.Panel1.Content = Me.samplesTree
            Me.sampleBox = New VBox()
            Me.title = New Label("Sample:")
            Me.sampleBox.PackStart(Me.title, BoxMode.None)
            box.Panel2.Content = Me.sampleBox
            box.Panel2.Resize = True
            box.Position = 160.0
            MyBase.Content = box
            AddHandler Me.samplesTree.SelectionChanged, AddressOf Me.HandleSamplesTreeSelectionChanged
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            MyBase.Dispose(disposing)
            If Me.statusIcon IsNot Nothing Then
                Me.statusIcon.Dispose()
            End If
        End Sub

        Private Sub HandleSamplesTreeSelectionChanged(sender As Object, e As EventArgs)
            If Me.samplesTree.SelectedRow IsNot Nothing Then
                If Me.currentSample IsNot Nothing Then
                    Me.sampleBox.Remove(Me.currentSample)
                End If
                Dim s As Sample = Me.store.GetNavigatorAt(Me.samplesTree.SelectedRow).GetValue(Of Sample)(Me.widgetCol)
                If s.Type IsNot Nothing Then
                    If s.Widget Is Nothing Then
                        s.Widget = CType(Activator.CreateInstance(s.Type), Widget)
                    End If
                    Me.sampleBox.PackStart(s.Widget, BoxMode.FillAndExpand)
                End If
                Me.currentSample = s.Widget
                Me.Dump(Me.currentSample, 0)
            End If
        End Sub

        Private Sub Dump(w As IWidgetSurface, ind As Integer)
            If w IsNot Nothing Then
                Console.WriteLine(New String(" ", ind * 2), " ", w.[GetType]().Name, " ", w.GetPreferredWidth(), " ", w.GetPreferredHeight())
                For Each c As Widget In w.Children
                    Me.Dump(c, ind + 1)
                Next
            End If
        End Sub

        Private Function AddSample(pos As TreePosition, name As String, sampleType As Type) As TreePosition
            Return Me.store.AddNode(pos).SetValue(Of String)(Me.nameCol, name).SetValue(Of Image)(Me.iconCol, Me.icon).SetValue(Of Sample)(Me.widgetCol, New Sample(sampleType)).CurrentPosition
        End Function
    End Class

    Friend Class Sample
        Public Type As Type

        Public Widget As Widget

        Public Sub New(type As Type)
            Me.Type = type
        End Sub
    End Class

End Namespace
