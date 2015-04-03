﻿Imports StaxRip.UI
Imports System.ComponentModel

Class SimpleUI
    Inherits Control

    Public WithEvents Tree As New TreeViewEx

    Public Host As New Panel

    Public Event SaveValues()
    Public Event ValueChanged()

    Public Pages As New List(Of IPage)

    Public ActivePage As IPage

    Sub New()
        InitControls()
        SetStyle(ControlStyles.SupportsTransparentBackColor, True)
    End Sub

    Sub InitControls()
        Tree.Scrollable = False
        Tree.SelectOnMouseDown = True
        Tree.ItemHeight = FontHeight * 2
        Tree.ShowLines = False
        Tree.HideSelection = False
        Tree.FullRowSelect = True
        Tree.ShowPlusMinus = False
        Tree.AutoCollaps = True
        Tree.ExpandMode = TreeNodeExpandMode.InclusiveChilds

        Controls.Add(Tree)
        Controls.Add(Host)
    End Sub

    Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
        Tree.Top = 0
        Tree.Left = 0
        Tree.Width = 0
        Tree.Height = Height

        If Tree.Nodes.Count > 1 Then
            Tree.Width = (Aggregate i In Tree.GetNodes Into Max(i.Bounds.Right)) + FontHeight
        End If

        Host.Top = 0
        Host.Left = Tree.Right + 8
        Host.Height = Height
        Host.Width = Width - Tree.Width - 8

        MyBase.OnLayout(levent)
    End Sub

    Sub Save()
        RaiseEvent SaveValues()
    End Sub

    Sub RaiseChangeEvent()
        RaiseEvent ValueChanged()
    End Sub

    Private Sub Tree_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles Tree.AfterSelect
        For Each i In Pages
            If Not i.Node Is e.Node Then
                DirectCast(i, Control).Visible = False
            End If
        Next

        For Each i In Pages
            If i.Node Is e.Node Then
                DirectCast(i, Control).Visible = True
                DirectCast(i, Control).BringToFront()
                ActivePage = i
                PerformLayout()
            End If
        Next
    End Sub

    Sub ShowPage(pagePath As String)
        For Each i In Pages
            If i.Path = pagePath Then
                Tree.SelectedNode = i.Node
            End If
        Next
    End Sub

    Sub ShowPage(page As IPage)
        For Each i In Pages
            If page Is i Then
                Tree.SelectedNode = i.Node
            End If
        Next
    End Sub

    Sub SelectLast(id As String)
        Dim last = s.Storage.GetString(id)

        If last <> "" Then
            ShowPage(last)
        ElseIf Pages.Count > 0 Then
            ShowPage(Pages(0))
        End If
    End Sub

    Sub SaveLast(id As String)
        If Not ActivePage Is Nothing Then
            s.Storage.SetString(id, ActivePage.Path)
        End If
    End Sub

    Function GetFlowPage(path As String) As FlowPage
        If path = "" Then path = "unknown"
        Dim q = From i In Pages Where i.Path = path

        If q.Count > 0 Then
            Return DirectCast(q.First, FlowPage)
        Else
            Return CreateFlowPage(path)
        End If
    End Function

    Function AddCheckBox(parent As FlowLayoutPanelEx) As SimpleUICheckBox
        Dim ret As New SimpleUICheckBox(Me)
        AddHandler SaveValues, AddressOf ret.Save
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddEdit(parent As FlowLayoutPanelEx) As SimpleUITextEdit
        Dim ret As New SimpleUITextEdit(Me)
        ret.Height = 36
        ret.Width = FontHeight * 10
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddNumeric(parent As FlowLayoutPanelEx) As SimpleUINumEdit
        Dim ret As New SimpleUINumEdit(Me)
        ret.Height = 36
        ret.Width = FontHeight * 4
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddLabel(parent As FlowLayoutPanelEx,
                      text As String,
                      Optional widthInFontHeights As Integer = 0) As SimpleUILabel

        Dim ret As New SimpleUILabel

        ret.Offset = widthInFontHeights
        ret.Text = text

        parent.Controls.Add(ret)

        Return ret
    End Function

    Function AddNumericBlock(parent As FlowLayoutPanelEx) As NumericBlock
        Dim ret As New NumericBlock(Me)
        ret.AutoSize = True
        ret.UseParenWidth = True
        AddHandler SaveValues, AddressOf ret.NumEdit.Save
        parent.Controls.Add(ret)
        Return ret
    End Function

    'TODO: remove?
    'Function AddCheckedNumericBlock(parent As FlowLayoutPanelEx) As CheckedNumericBlock
    '    Dim ret As New CheckedNumericBlock(Me)
    '    ret.AutoSize = True
    '    ret.UseParenWidth = True
    '    AddHandler SaveValues, AddressOf ret.NumEdit.Save
    '    AddHandler SaveValues, AddressOf ret.CheckBox.Save
    '    parent.Controls.Add(ret)
    '    Return ret
    'End Function

    Function AddTextBlock(parent As FlowLayoutPanelEx) As TextBlock
        Dim ret As New TextBlock(Me)
        ret.AutoSize = True
        ret.UseParenWidth = True
        AddHandler SaveValues, AddressOf ret.Edit.Save
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddTextMenuBlock(parent As FlowLayoutPanelEx) As TextMenuBlock
        Dim ret As New TextMenuBlock(Me)
        ret.AutoSize = True
        ret.UseParenWidth = True
        AddHandler SaveValues, AddressOf ret.Edit.Save
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddTextButtonBlock(parent As FlowLayoutPanelEx) As TextButtonBlock
        Dim ret As New TextButtonBlock(Me)
        ret.AutoSize = True
        ret.UseParenWidth = True
        AddHandler SaveValues, AddressOf ret.Edit.Save
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddMenuButtonBlock(Of T)(parent As FlowLayoutPanelEx) As MenuButtonBlock(Of T)
        Dim ret As New MenuButtonBlock(Of T)(Me)
        ret.AutoSize = True
        ret.UseParenWidth = True
        AddHandler SaveValues, AddressOf ret.MenuButton.Save
        parent.Controls.Add(ret)
        Return ret
    End Function

    Function AddEmptyBlock(parent As FlowLayoutPanelEx) As EmptyBlock
        Dim ret As New EmptyBlock
        ret.AutoSize = True
        ret.UseParenWidth = True
        parent.Controls.Add(ret)
        Return ret
    End Function

    Sub AddLine(parent As FlowLayoutPanelEx, text As String)
        Dim line As New LineControl
        line.Height = 36
        line.Width = 100
        line.Text = text
        parent.ExpandedControls(line) = True
        parent.Controls.Add(line)
    End Sub

    Function CreateFlowPage(path As String) As FlowPage
        Dim ret = New FlowPage
        Pages.Add(ret)
        ret.Path = path
        ret.Dock = DockStyle.Fill
        ret.Node = Tree.AddNode(path)
        Host.Controls.Add(ret)
        ActivePage = ret
        Return ret
    End Function

    Sub CreateControlPage(ctrl As IPage, path As String)
        Pages.Add(ctrl)
        ctrl.Path = path
        DirectCast(ctrl, Control).Dock = DockStyle.Fill
        ctrl.Node = Tree.AddNode(path)
        Host.Controls.Add(DirectCast(ctrl, Control))
    End Sub

    Function CreateDataPage(path As String) As DataPage
        Dim ret = New DataPage
        ret.EditMode = DataGridViewEditMode.EditOnEnter
        ret.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        ret.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        ret.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Pages.Add(ret)
        ret.Path = Path
        ret.Dock = DockStyle.Fill
        ret.Node = Tree.AddNode(Path)
        Host.Controls.Add(ret)
        ActivePage = ret
        Return ret
    End Function

    Class DataPage
        Inherits DataGridViewEx
        Implements IPage

        Public Property Node As TreeNode Implements IPage.Node
        Public Property Path As String Implements IPage.Path
        Public Property TipProvider As TipProvider Implements IPage.TipProvider

        Sub New()
            TipProvider = New TipProvider(Nothing)
            AddHandler Disposed, Sub() TipProvider.Dispose()
        End Sub
    End Class

    Class FlowPage
        Inherits FlowLayoutPanelEx
        Implements IPage

        Property Node As TreeNode Implements IPage.Node
        Property Path As String Implements IPage.Path
        Property TipProvider As TipProvider Implements IPage.TipProvider

        Sub New()
            TipProvider = New TipProvider(Nothing)
            AddHandler Disposed, Sub() TipProvider.Dispose()

            FlowDirection = FlowDirection.TopDown
        End Sub
    End Class

    Class SimpleUICheckBox
        Inherits CheckBoxEx

        Property SaveAction As Action(Of Boolean)

        Private SimpleUI As SimpleUI

        Sub New(ui As SimpleUI)
            AutoSize = True
            SimpleUI = ui
        End Sub

        Protected Overrides Sub OnCheckedChanged(e As EventArgs)
            MyBase.OnCheckedChanged(e)
            SimpleUI.RaiseChangeEvent()
        End Sub

        WriteOnly Property Tooltip As String
            Set(value As String)
                Dim parent = Me.Parent

                While Not TypeOf parent Is IPage
                    parent = parent.Parent
                End While

                DirectCast(parent, IPage).TipProvider.SetTip(value, Me)
            End Set
        End Property

        WriteOnly Property MarginTop As Integer
            Set(value As Integer)
                Dim m = Margin
                m.Top = value
                Margin = m
            End Set
        End Property

        Private OffsetValue As Integer

        Property Offset As Integer
            Get
                Return OffsetValue
            End Get
            Set(value As Integer)
                OffsetValue = value
                PerformLayout()
            End Set
        End Property

        Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
            If Offset > 0 Then
                Dim ret = MyBase.GetPreferredSize(proposedSize)

                If ret.Width < Offset * FontHeight - 6 Then
                    ret.Width = Offset * FontHeight - 6
                End If

                Return ret
            Else
                Return MyBase.GetPreferredSize(proposedSize)
            End If
        End Function

        Sub Save()
            SaveAction.Invoke(Checked)
        End Sub
    End Class

    Class SimpleUINumEdit
        Inherits NumEdit

        Private SimpleUI As SimpleUI

        Property SaveAction As Action(Of Decimal)

        Sub New(ui As SimpleUI)
            SimpleUI = ui
        End Sub

        Sub Save()
            SaveAction.Invoke(Value)
        End Sub

        Sub Init(ParamArray params As Decimal())
            If Not params Is Nothing Then
                If params.Length > 0 Then Minimum = params(0)
                If params.Length > 1 Then Maximum = params(1)
                If params.Length > 2 Then Increment = params(2)
                If params.Length > 3 Then DecimalPlaces = CInt(params(3))
            End If
        End Sub

        Protected Overrides Sub OnValueChanged(numEdit As NumEdit)
            MyBase.OnValueChanged(numEdit)
            SimpleUI.RaiseChangeEvent()
        End Sub
    End Class

    Class SimpleUITextEdit
        Inherits TextEdit

        Property SaveAction As Action(Of String)

        Private SimpleUI As SimpleUI

        Sub New(ui As SimpleUI)
            SimpleUI = ui
            AddHandler TextBox.TextChanged, Sub() SimpleUI.RaiseChangeEvent()
            AddHandler SimpleUI.SaveValues, AddressOf Save
        End Sub

        Sub Save()
            SaveAction.Invoke(Text)
        End Sub

        WriteOnly Property UseCommandlineEditor As Boolean
            Set(value As Boolean)
                If value Then
                    AddHandler TextBox.Click, Sub() EditCommandline()
                End If
            End Set
        End Property

        WriteOnly Property UseMacroEditor As Boolean
            Set(value As Boolean)
                If value Then AddHandler TextBox.Click, Sub() EditMacro()
            End Set
        End Property

        Sub EditCommandline()
            Using f As New MacroEditor
                f.SetCommandlineDefaults()
                f.MacroEditorControl.Value = Text

                If f.ShowDialog() = DialogResult.OK Then
                    Text = f.MacroEditorControl.Value
                End If
            End Using
        End Sub

        Sub EditMacro()
            Using f As New MacroEditor
                f.SetMacroDefaults()
                f.MacroEditorControl.Value = Text

                If f.ShowDialog() = DialogResult.OK Then
                    Text = f.MacroEditorControl.Value
                End If
            End Using
        End Sub

        Sub BrowseFile(filter As String)
            Using d As New OpenFileDialog
                d.Filter = filter

                If File.Exists(Text) Then
                    d.FileName = Text
                End If

                If d.ShowDialog = DialogResult.OK Then
                    Text = d.FileName
                End If
            End Using
        End Sub
    End Class

    Class SimpleUIMenuButton(Of T)
        Inherits MenuButton

        Property SaveAction As Action(Of T)

        Private SimpleUI As SimpleUI

        Sub New(ui As SimpleUI)
            SimpleUI = ui
        End Sub

        Protected Overrides Sub OnValueChanged(value As Object)
            MyBase.OnValueChanged(value)
            SimpleUI.RaiseChangeEvent()
        End Sub

        Shadows Property Value As T
            Get
                Return CType(MyBase.Value, T)
            End Get
            Set(value As T)
                MyBase.Value = CType(value, T)
            End Set
        End Property

        Sub Save()
            SaveAction.Invoke(Value)
        End Sub
    End Class

    Class SimpleUILabel
        Inherits LabelEx

        Private OffsetValue As Integer

        Property Offset As Integer
            Get
                Return OffsetValue
            End Get
            Set(value As Integer)
                OffsetValue = value
                PerformLayout()
            End Set
        End Property

        Sub New()
            TextAlign = ContentAlignment.MiddleLeft
            AutoSize = True
        End Sub

        WriteOnly Property Tooltip As String
            Set(value As String)
                Dim parent = Me.Parent

                While Not TypeOf parent Is IPage
                    parent = parent.Parent
                End While

                DirectCast(parent, IPage).TipProvider.SetTip(value, Me)
            End Set
        End Property

        WriteOnly Property MarginTop As Integer
            Set(value As Integer)
                Dim m = Margin
                m.Top = value
                Margin = m
            End Set
        End Property

        WriteOnly Property MarginLeft As Integer
            Set(value As Integer)
                Dim m = Margin
                m.Left = value
                Margin = m
            End Set
        End Property

        Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
            If Offset > 0 Then
                Dim ret = MyBase.GetPreferredSize(proposedSize)

                If ret.Width < Offset * FontHeight Then
                    ret.Width = Offset * FontHeight
                End If

                Return ret
            Else
                Return MyBase.GetPreferredSize(proposedSize)
            End If
        End Function
    End Class

    Class EmptyBlock
        Inherits FlowLayoutPanelEx

        Sub New()
            Font = New Font("Segoe UI", 9.0!)
        End Sub

        Protected Overrides Sub OnControlAdded(e As ControlEventArgs)
            Height = Aggregate i In Controls.OfType(Of Control)()
                     Into Max(i.Height + i.Margin.Top + i.Margin.Bottom)

            MyBase.OnControlAdded(e)
        End Sub
    End Class

    MustInherit Class LabelBlock
        Inherits EmptyBlock

        Property Label As New SimpleUILabel

        Sub New()
            Controls.Add(Label)
        End Sub
    End Class

    Class NumericBlock
        Inherits LabelBlock

        Property NumEdit As SimpleUINumEdit

        Sub New(ui As SimpleUI)
            NumEdit = New SimpleUINumEdit(ui)
            NumEdit.Height = 36
            NumEdit.Width = 100
            Controls.Add(NumEdit)
        End Sub
    End Class

    'TODO: remove?
    'Class CheckedNumericBlock
    '    Inherits EmptyBlock

    '    Property NumEdit As SimpleUINumEdit
    '    Property CheckBox As SimpleUICheckBox

    '    Sub New(ui As SimpleUI)
    '        CheckBox = New SimpleUICheckBox(ui)
    '        Controls.Add(CheckBox)
    '        NumEdit = New SimpleUINumEdit(ui)
    '        NumEdit.Height = 36
    '        NumEdit.Width = 100
    '        Controls.Add(NumEdit)
    '    End Sub
    'End Class

    Class TextBlock
        Inherits LabelBlock

        Property Edit As SimpleUITextEdit

        Sub New(ui As SimpleUI)
            Edit = New SimpleUITextEdit(ui)
            Edit.Height = 36
            Edit.Width = 250
            Controls.Add(Edit)
        End Sub
    End Class

    Class TextMenuBlock
        Inherits TextBlock

        Property Button As New ButtonEx

        Sub New(ui As SimpleUI)
            MyBase.New(ui)
            Button.Width = 36
            Button.Height = 36
            Button.ShowMenuSymbol = True
            Button.ContextMenuStrip = New ContextMenuStrip

            Controls.Add(Button)
        End Sub

        Sub AddMenu(menuText As String, menuValue As String)
            AddMenu(menuText, Function() menuValue)
        End Sub

        Sub AddMenu(menuText As String, menuFunc As Func(Of String))
            Dim action = Sub()
                             Dim v = menuFunc.Invoke
                             If OK(v) Then Edit.Text = v
                         End Sub

            AddMenu(menuText, action)
        End Sub

        Sub AddMenu(menuText As String, menuAction As action)
            ActionMenuItem.Add(Button.ContextMenuStrip.Items, menuText, menuAction)
        End Sub
    End Class

    Class TextButtonBlock
        Inherits TextBlock

        Property Button As New ButtonEx

        Sub New(ui As SimpleUI)
            MyBase.New(ui)
            Button.Width = 36
            Button.Height = 36
            Button.AutoSize = True

            Controls.Add(Button)
        End Sub
    End Class

    Class MenuButtonBlock(Of T)
        Inherits LabelBlock

        Property MenuButton As SimpleUIMenuButton(Of T)

        Sub New(ui As SimpleUI)
            MenuButton = New SimpleUIMenuButton(Of T)(ui)

            MenuButton.Height = 36
            MenuButton.Width = 250

            Controls.Add(MenuButton)
        End Sub

        Public WriteOnly Property Tooltip As String
            Set(value As String)
                Dim parent = Me.Parent

                While Not TypeOf parent Is IPage
                    parent = parent.Parent
                End While

                DirectCast(parent, IPage).TipProvider.SetTip(value, Label, MenuButton)
            End Set
        End Property
    End Class

    'Function GetColumn(path As String) As FlowLayoutPanelEx
    '    Dim column = path.Substring(path.Length - 1).ToInt
    '    path = path.Substring(0, path.Length - 2)
    '    Dim query = From v In Pages Where v.Path = path

    '    If query.Count = 0 Then
    '        Dim page = New TwoColumnsPage
    '        Pages.Add(page)
    '        page.Path = path
    '        page.Dock = DockStyle.Fill
    '        page.Node = Tree.AddNode(path)
    '        Parent.Controls.Add(page)
    '        ActivePage = page
    '        Return DirectCast(page.Controls(column), FlowLayoutPanelEx)
    '    Else
    '        Dim page = query.First
    '        ActivePage = page
    '        Dim control = DirectCast(page, Control)
    '        control.BringToFront()
    '        Return DirectCast(control.Controls(column), FlowLayoutPanelEx)
    '    End If
    'End Function

    'Class TwoColumnsPage
    '    Inherits Control
    '    Implements IPage

    '    Public Property Node As TreeNode Implements IPage.Node
    '    Public Property Path As String Implements IPage.Path
    '    Public Property TipProvider As TipProvider Implements IPage.TipProvider

    '    Sub New()
    '        TipProvider = New TipProvider(Nothing)
    '        AddHandler Disposed, Sub() TipProvider.Dispose()
    '        Controls.Add(New FlowLayoutPanelEx)
    '        Controls.Add(New FlowLayoutPanelEx)
    '    End Sub

    '    Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
    '        If Controls.Count = 2 Then
    '            Dim a = Controls(0)
    '            Dim b = Controls(1)

    '            a.Left = 0
    '            a.Top = 0
    '            a.Width = CInt(Width / 2)
    '            a.Height = Height

    '            b.Left = a.Width
    '            b.Top = 0
    '            b.Width = a.Width
    '            b.Height = Height
    '        End If

    '        MyBase.OnLayout(levent)
    '    End Sub
    'End Class
End Class