Imports System
Imports System.Threading
Imports System.Windows.Controls
Imports Microsoft.Phone.Controls
Imports Microsoft.Phone.Shell
Imports System.Windows.Media.Imaging
Imports Microsoft.Phone.PictureDecoder

Partial Public Class MainPage
    Inherits PhoneApplicationPage

    Private bWhite As Brush, bBlack As Brush, bGreen As Brush
    Private ColorCache As WriteableBitmap
    Private RowSpan As Integer, ColSpan As Integer
    Private SettingsChanged As Boolean
    Private DrawMode As TileModeTypes
    Private altinvert As Boolean

    ' Fill preview array
    Private Sub RefreshPreviewCache(rct_y As Integer, rct_x As Integer, clr As Integer)
        Dim xref As Integer, yref As Integer
        Dim K As Integer, L As Integer, _k As Integer, _l As Integer
        Dim new_x As Integer, new_y As Integer
        Dim dx As Integer, dy As Integer
        Dim xblk As Integer, yblk As Integer
        Dim M As Integer, N As Integer

        M = 8
        N = 8
        K = (ColorCache.PixelWidth / M)
        L = (ColorCache.PixelHeight / N)
        _k = RowSpan
        _l = ColSpan

        For xblk = 0 To K / _k - 1
            For yblk = 0 To L / _l - 1
                xref = xblk * (_k * M)
                yref = yblk * (_l * N)

                For dx = 0 To _k - 1
                    For dy = 0 To _l - 1
                        new_x = xref + _k * rct_x + dx
                        new_y = yref + _l * rct_y + dy
                        ColorCache.Pixels(new_x + new_y * (K * M)) = clr
                    Next
                Next
            Next
        Next
    End Sub

    Private Sub RedrawTileCanvasToCache()
        Dim rect As Rectangle
        Dim row As Integer, col As Integer
        Dim clr As Integer

        For row = 0 To 7
            For col = 0 To 7
                rect = FindName("rct" + Convert.ToString(row) + "_" + Convert.ToString(col))
                If Convert.ToInt32(rect.Tag) = 1 Then
                    clr = &HFFFFFFFF
                Else
                    clr = &HFF000000
                End If
                RefreshPreviewCache(row, col, clr)
            Next
        Next

        SettingsChanged = True
    End Sub

    Private Sub RefreshPreviewImage()
        ColorCache.invalidate()
        imgPreview.Source = ColorCache
    End Sub

    Private Function ToggleTile(rect As Rectangle) As Integer
        Dim clr As Integer

        If Convert.ToInt32(rect.Tag) = 1 Then
            rect.Tag = "0"
            rect.Fill = bBlack
            clr = &HFF000000
        Else
            rect.Tag = "1"
            rect.Fill = bWhite
            clr = &HFFFFFFFF
        End If

        ToggleTile = clr

        SettingsChanged = True
    End Function

    Private Sub InvertTileCanvasAndPreview()
        Dim rect As Rectangle
        Dim row As Integer, col As Integer
        Dim clr As Integer

        For row = 0 To 7
            For col = 0 To 7
                rect = FindName("rct" + Convert.ToString(row) + "_" + Convert.ToString(col))
                ToggleTile(rect)
            Next
        Next

        ' Invert the preview cache, we do a XOR invert here
        For row = 0 To ColorCache.PixelHeight - 1
            For col = 0 To ColorCache.PixelWidth - 1
                clr = ColorCache.Pixels(row * ColorCache.PixelWidth + col)
                clr = clr Xor &HFFFFFF
                ColorCache.Pixels(row * ColorCache.PixelWidth + col) = clr
            Next
        Next

        SettingsChanged = True
    End Sub

    Private Sub ClearTileCanvasAndPreview(invert As Boolean)
        Dim rect As Rectangle
        Dim tag As String, fill As Brush
        Dim row As Integer, col As Integer
        Dim clr As Integer

        If invert = True Then
            tag = "1"
            fill = bWhite
            clr = &HFFFFFFFF
        Else
            tag = "0"
            fill = bBlack
            clr = &HFF000000
        End If

        For row = 0 To 7
            For col = 0 To 7
                rect = FindName("rct" + Convert.ToString(row) + "_" + Convert.ToString(col))
                rect.Tag = tag
                rect.Fill = fill
                rect.Stroke = bWhite
            Next
        Next

        ' Clear the preview cache
        For row = 0 To ColorCache.PixelHeight - 1
            For col = 0 To ColorCache.PixelWidth - 1
                ColorCache.Pixels(row * ColorCache.PixelWidth + col) = clr
            Next
        Next

        SettingsChanged = True
    End Sub

    Private Sub SelectPaintOption(opt As Integer)
        Dim txtList(8) As TextBlock
        Dim i As Integer

        txtList(0) = txtNormal
        txtList(1) = txtMirror
        txtList(2) = txtClkRotate
        txtList(3) = txtMirrorRotate
        txtList(4) = txtRotateMirror
        txtList(5) = txtRoll
        txtList(6) = txtTilePoly

        For i = 0 To 6
            txtList(i).Foreground = bWhite
        Next
        txtList(opt).Foreground = bGreen

        DrawMode = opt
        SettingsChanged = True
    End Sub


    ' Constructor
    Public Sub New()
        InitializeComponent()

        SupportedOrientations = SupportedPageOrientation.Portrait Or SupportedPageOrientation.Landscape

        ' Sample code to localize the ApplicationBar
        'BuildLocalizedApplicationBar()

        bWhite = New SolidColorBrush(Color.FromArgb(&HFF, &HFF, &HFF, &HFF))
        bBlack = New SolidColorBrush(Color.FromArgb(&HFF, 0, 0, 0))
        bGreen = New SolidColorBrush(Color.FromArgb(&HFF, 0, &HFF, 0))

        ' Row/Column spans: Each 1x1 tile spans about 4x4 pixels in
        ' the final image
        RowSpan = 2 ^ Convert.ToInt32(sldSpanY.Value)
        ColSpan = 2 ^ Convert.ToInt32(sldSpanX.Value)

        ' Dirtify settings
        SettingsChanged = True

        ' Create ColorCache
        ColorCache = New WriteableBitmap(imgPreview.Width, imgPreview.Height)

        ' Load rectangle with black color
        ClearTileCanvasAndPreview(False)

        ' Select Normal paint option
        SelectPaintOption(TileModeTypes.Normal)

        ' Refresh preview image
        RefreshPreviewImage()
    End Sub

    ' Sample code for building a localized ApplicationBar
    'Private Sub BuildLocalizedApplicationBar()
    '    ' Set the page's ApplicationBar to a new instance of ApplicationBar.
    '    ApplicationBar = New ApplicationBar()

    '    ' Create a new button and set the text value to the localized string from AppResources.
    '    Dim appBarButton As New ApplicationBarIconButton(New Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative))
    '    appBarButton.Text = AppResources.AppBarButtonText
    '    ApplicationBar.Buttons.Add(appBarButton)

    '    ' Create a new menu item with the localized string from AppResources.
    '    Dim appBarMenuItem As New ApplicationBarMenuItem(AppResources.AppBarMenuItemText)
    '    ApplicationBar.MenuItems.Add(appBarMenuItem)
    'End Sub

    Private Sub rct0_0_Tap(sender As Object, e As GestureEventArgs) Handles rct0_0.Tap
        RefreshPreviewCache(0, 0, ToggleTile(rct0_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_0_Tap(sender As Object, e As GestureEventArgs) Handles rct1_0.Tap
        RefreshPreviewCache(1, 0, ToggleTile(rct1_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_0_Tap(sender As Object, e As GestureEventArgs) Handles rct2_0.Tap
        RefreshPreviewCache(2, 0, ToggleTile(rct2_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_0_Tap(sender As Object, e As GestureEventArgs) Handles rct3_0.Tap
        RefreshPreviewCache(3, 0, ToggleTile(rct3_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_0_Tap(sender As Object, e As GestureEventArgs) Handles rct4_0.Tap
        RefreshPreviewCache(4, 0, ToggleTile(rct4_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_0_Tap(sender As Object, e As GestureEventArgs) Handles rct5_0.Tap
        RefreshPreviewCache(5, 0, ToggleTile(rct5_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_0_Tap(sender As Object, e As GestureEventArgs) Handles rct6_0.Tap
        RefreshPreviewCache(6, 0, ToggleTile(rct6_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_0_Tap(sender As Object, e As GestureEventArgs) Handles rct7_0.Tap
        RefreshPreviewCache(7, 0, ToggleTile(rct7_0))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_1_Tap(sender As Object, e As GestureEventArgs) Handles rct0_1.Tap
        RefreshPreviewCache(0, 1, ToggleTile(rct0_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_1_Tap(sender As Object, e As GestureEventArgs) Handles rct1_1.Tap
        RefreshPreviewCache(1, 1, ToggleTile(rct1_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_1_Tap(sender As Object, e As GestureEventArgs) Handles rct2_1.Tap
        RefreshPreviewCache(2, 1, ToggleTile(rct2_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_1_Tap(sender As Object, e As GestureEventArgs) Handles rct3_1.Tap
        RefreshPreviewCache(3, 1, ToggleTile(rct3_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_1_Tap(sender As Object, e As GestureEventArgs) Handles rct4_1.Tap
        RefreshPreviewCache(4, 1, ToggleTile(rct4_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_1_Tap(sender As Object, e As GestureEventArgs) Handles rct5_1.Tap
        RefreshPreviewCache(5, 1, ToggleTile(rct5_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_1_Tap(sender As Object, e As GestureEventArgs) Handles rct6_1.Tap
        RefreshPreviewCache(6, 1, ToggleTile(rct6_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_1_Tap(sender As Object, e As GestureEventArgs) Handles rct7_1.Tap
        RefreshPreviewCache(7, 1, ToggleTile(rct7_1))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_2_Tap(sender As Object, e As GestureEventArgs) Handles rct0_2.Tap
        RefreshPreviewCache(0, 2, ToggleTile(rct0_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_2_Tap(sender As Object, e As GestureEventArgs) Handles rct1_2.Tap
        RefreshPreviewCache(1, 2, ToggleTile(rct1_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_2_Tap(sender As Object, e As GestureEventArgs) Handles rct2_2.Tap
        RefreshPreviewCache(2, 2, ToggleTile(rct2_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_2_Tap(sender As Object, e As GestureEventArgs) Handles rct3_2.Tap
        RefreshPreviewCache(3, 2, ToggleTile(rct3_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_2_Tap(sender As Object, e As GestureEventArgs) Handles rct4_2.Tap
        RefreshPreviewCache(4, 2, ToggleTile(rct4_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_2_Tap(sender As Object, e As GestureEventArgs) Handles rct5_2.Tap
        RefreshPreviewCache(5, 2, ToggleTile(rct5_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_2_Tap(sender As Object, e As GestureEventArgs) Handles rct6_2.Tap
        RefreshPreviewCache(6, 2, ToggleTile(rct6_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_2_Tap(sender As Object, e As GestureEventArgs) Handles rct7_2.Tap
        RefreshPreviewCache(7, 2, ToggleTile(rct7_2))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_3_Tap(sender As Object, e As GestureEventArgs) Handles rct0_3.Tap
        RefreshPreviewCache(0, 3, ToggleTile(rct0_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_3_Tap(sender As Object, e As GestureEventArgs) Handles rct1_3.Tap
        RefreshPreviewCache(1, 3, ToggleTile(rct1_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_3_Tap(sender As Object, e As GestureEventArgs) Handles rct2_3.Tap
        RefreshPreviewCache(2, 3, ToggleTile(rct2_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_3_Tap(sender As Object, e As GestureEventArgs) Handles rct3_3.Tap
        RefreshPreviewCache(3, 3, ToggleTile(rct3_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_3_Tap(sender As Object, e As GestureEventArgs) Handles rct4_3.Tap
        RefreshPreviewCache(4, 3, ToggleTile(rct4_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_3_Tap(sender As Object, e As GestureEventArgs) Handles rct5_3.Tap
        RefreshPreviewCache(5, 3, ToggleTile(rct5_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_3_Tap(sender As Object, e As GestureEventArgs) Handles rct6_3.Tap
        RefreshPreviewCache(6, 3, ToggleTile(rct6_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_3_Tap(sender As Object, e As GestureEventArgs) Handles rct7_3.Tap
        RefreshPreviewCache(7, 3, ToggleTile(rct7_3))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_4_Tap(sender As Object, e As GestureEventArgs) Handles rct0_4.Tap
        RefreshPreviewCache(0, 4, ToggleTile(rct0_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_4_Tap(sender As Object, e As GestureEventArgs) Handles rct1_4.Tap
        RefreshPreviewCache(1, 4, ToggleTile(rct1_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_4_Tap(sender As Object, e As GestureEventArgs) Handles rct2_4.Tap
        RefreshPreviewCache(2, 4, ToggleTile(rct2_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_4_Tap(sender As Object, e As GestureEventArgs) Handles rct3_4.Tap
        RefreshPreviewCache(3, 4, ToggleTile(rct3_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_4_Tap(sender As Object, e As GestureEventArgs) Handles rct4_4.Tap
        RefreshPreviewCache(4, 4, ToggleTile(rct4_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_4_Tap(sender As Object, e As GestureEventArgs) Handles rct5_4.Tap
        RefreshPreviewCache(5, 4, ToggleTile(rct5_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_4_Tap(sender As Object, e As GestureEventArgs) Handles rct6_4.Tap
        RefreshPreviewCache(6, 4, ToggleTile(rct6_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_4_Tap(sender As Object, e As GestureEventArgs) Handles rct7_4.Tap
        RefreshPreviewCache(7, 4, ToggleTile(rct7_4))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_5_Tap(sender As Object, e As GestureEventArgs) Handles rct0_5.Tap
        RefreshPreviewCache(0, 5, ToggleTile(rct0_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_5_Tap(sender As Object, e As GestureEventArgs) Handles rct1_5.Tap
        RefreshPreviewCache(1, 5, ToggleTile(rct1_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_5_Tap(sender As Object, e As GestureEventArgs) Handles rct2_5.Tap
        RefreshPreviewCache(2, 5, ToggleTile(rct2_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_5_Tap(sender As Object, e As GestureEventArgs) Handles rct3_5.Tap
        RefreshPreviewCache(3, 5, ToggleTile(rct3_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_5_Tap(sender As Object, e As GestureEventArgs) Handles rct4_5.Tap
        RefreshPreviewCache(4, 5, ToggleTile(rct4_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_5_Tap(sender As Object, e As GestureEventArgs) Handles rct5_5.Tap
        RefreshPreviewCache(5, 5, ToggleTile(rct5_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_5_Tap(sender As Object, e As GestureEventArgs) Handles rct6_5.Tap
        RefreshPreviewCache(6, 5, ToggleTile(rct6_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_5_Tap(sender As Object, e As GestureEventArgs) Handles rct7_5.Tap
        RefreshPreviewCache(7, 5, ToggleTile(rct7_5))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_6_Tap(sender As Object, e As GestureEventArgs) Handles rct0_6.Tap
        RefreshPreviewCache(0, 6, ToggleTile(rct0_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_6_Tap(sender As Object, e As GestureEventArgs) Handles rct1_6.Tap
        RefreshPreviewCache(1, 6, ToggleTile(rct1_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_6_Tap(sender As Object, e As GestureEventArgs) Handles rct2_6.Tap
        RefreshPreviewCache(2, 6, ToggleTile(rct2_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_6_Tap(sender As Object, e As GestureEventArgs) Handles rct3_6.Tap
        RefreshPreviewCache(3, 6, ToggleTile(rct3_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_6_Tap(sender As Object, e As GestureEventArgs) Handles rct4_6.Tap
        RefreshPreviewCache(4, 6, ToggleTile(rct4_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_6_Tap(sender As Object, e As GestureEventArgs) Handles rct5_6.Tap
        RefreshPreviewCache(5, 6, ToggleTile(rct5_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_6_Tap(sender As Object, e As GestureEventArgs) Handles rct6_6.Tap
        RefreshPreviewCache(6, 6, ToggleTile(rct6_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_6_Tap(sender As Object, e As GestureEventArgs) Handles rct7_6.Tap
        RefreshPreviewCache(7, 6, ToggleTile(rct7_6))
        RefreshPreviewImage()
    End Sub

    Private Sub rct0_7_Tap(sender As Object, e As GestureEventArgs) Handles rct0_7.Tap
        RefreshPreviewCache(0, 7, ToggleTile(rct0_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct1_7_Tap(sender As Object, e As GestureEventArgs) Handles rct1_7.Tap
        RefreshPreviewCache(1, 7, ToggleTile(rct1_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct2_7_Tap(sender As Object, e As GestureEventArgs) Handles rct2_7.Tap
        RefreshPreviewCache(2, 7, ToggleTile(rct2_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct3_7_Tap(sender As Object, e As GestureEventArgs) Handles rct3_7.Tap
        RefreshPreviewCache(3, 7, ToggleTile(rct3_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct4_7_Tap(sender As Object, e As GestureEventArgs) Handles rct4_7.Tap
        RefreshPreviewCache(4, 7, ToggleTile(rct4_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct5_7_Tap(sender As Object, e As GestureEventArgs) Handles rct5_7.Tap
        RefreshPreviewCache(5, 7, ToggleTile(rct5_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct6_7_Tap(sender As Object, e As GestureEventArgs) Handles rct6_7.Tap
        RefreshPreviewCache(6, 7, ToggleTile(rct6_7))
        RefreshPreviewImage()
    End Sub

    Private Sub rct7_7_Tap(sender As Object, e As GestureEventArgs) Handles rct7_7.Tap
        RefreshPreviewCache(7, 7, ToggleTile(rct7_7))
        RefreshPreviewImage()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As RoutedEventArgs) Handles btnClear.Click
        ' Load rectangle with black color
        ClearTileCanvasAndPreview(False)

        ' Refresh preview image
        RefreshPreviewImage()
    End Sub

    Private Sub btnInvert_Click(sender As Object, e As RoutedEventArgs) Handles btnInvert.Click
        InvertTileCanvasAndPreview()

        ' Refresh preview image
        RefreshPreviewImage()
    End Sub

    Private Sub sldSpanX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldSpanX.ValueChanged
        If Convert.ToInt32(e.NewValue) <> Convert.ToInt32(e.OldValue) Then
            ColSpan = 2 ^ Convert.ToInt32(e.NewValue)

            ' Draw tile canvas to color cache
            RedrawTileCanvasToCache()

            ' Refresh preview image
            RefreshPreviewImage()

        End If
    End Sub

    Private Sub sldSpanY_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldSpanY.ValueChanged
        If Convert.ToInt32(e.NewValue) <> Convert.ToInt32(e.OldValue) Then
            RowSpan = 2 ^ Convert.ToInt32(e.NewValue)

            ' Draw tile canvas to color cache
            RedrawTileCanvasToCache()

            ' Refresh preview image
            RefreshPreviewImage()

        End If
    End Sub

    Private Sub imgPreview_Tap(sender As Object, e As GestureEventArgs) Handles imgPreview.Tap
        Dim param_str As String, str As String
        Dim row As Integer, col As Integer
        Dim rect As Rectangle

        param_str = ""
        If SettingsChanged = False Then
            param_str = "redraw=true"
        Else
            param_str = param_str + "rowcount=" + Convert.ToString(8) + "&"
            param_str = param_str + "colcount=" + Convert.ToString(8) + "&"
            For row = 0 To 7
                str = ""
                For col = 0 To 7
                    rect = FindName("rct" + Convert.ToString(row) + "_" + Convert.ToString(col))
                    str = str + rect.Tag.ToString
                Next
                param_str = param_str + "row" + Convert.ToString(row) + "=" + str + "&"
            Next
            param_str = param_str + "rowspan=" + Convert.ToString(RowSpan) + "&"
            param_str = param_str + "colspan=" + Convert.ToString(ColSpan) + "&"
            param_str = param_str + "drawmode=" + Convert.ToString(DrawMode)
            If AltInvert = True Then
                param_str = param_str + "&" + "altinvert=1"
            End If
        End If

        SettingsChanged = False
        NavigationService.Navigate(New Uri("/TileCanvas.xaml?" + param_str, UriKind.Relative))
    End Sub

    Private Sub txtMirror_Tap(sender As Object, e As GestureEventArgs) Handles txtMirror.Tap
        SelectPaintOption(TileModeTypes.Mirror)
    End Sub

    Private Sub txtNormal_Tap(sender As Object, e As GestureEventArgs) Handles txtNormal.Tap
        SelectPaintOption(TileModeTypes.Normal)
    End Sub

    Private Sub txtClkRotate_Tap(sender As Object, e As GestureEventArgs) Handles txtClkRotate.Tap
        SelectPaintOption(TileModeTypes.RotateClock)
    End Sub

    Private Sub txtAltInvert_Hold(sender As Object, e As GestureEventArgs) Handles txtAltInvert.Hold
        If AltInvert = True Then
            AltInvert = False
            txtAltInvert.Foreground = bWhite
            brdAltInvert.Background = bBlack
        Else
            AltInvert = True
            txtAltInvert.Foreground = bBlack
            brdAltInvert.Background = bWhite
        End If
        SettingsChanged = True
    End Sub

    Private Sub txtMirrorRotate_Tap(sender As Object, e As GestureEventArgs) Handles txtMirrorRotate.Tap
        SelectPaintOption(TileModeTypes.MirrorRotateClock)
    End Sub

    Private Sub txtRoll_Tap(sender As Object, e As GestureEventArgs) Handles txtRoll.Tap
        SelectPaintOption(TileModeTypes.Roll)
    End Sub

    Private Sub txtRotateMirror_Tap(sender As Object, e As GestureEventArgs) Handles txtRotateMirror.Tap
        SelectPaintOption(TileModeTypes.RotateClockMirror)
    End Sub

    Private Sub txtTilePoly_Tap(sender As Object, e As GestureEventArgs) Handles txtTilePoly.Tap
        SelectPaintOption(TileModeTypes.Polyomino)
    End Sub
End Class