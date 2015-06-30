Imports System
Imports System.Threading
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
Imports Microsoft.Phone.Controls
Imports Microsoft.Phone.Shell
Imports Microsoft.Phone.PictureDecoder

Partial Public Class MainPage
    Inherits PhoneApplicationPage

    Private commonSliderMode As Boolean

    Private Sub FlipBitTag(rbit As Rectangle)
        If Convert.ToInt32(rbit.Tag) = 0 Then
            rbit.Fill = New SolidColorBrush(Color.FromArgb(&HFF, &HFF, &HFF, &HFF))
            rbit.Tag = 1
        Else
            rbit.Fill = New SolidColorBrush(Color.FromArgb(&HFF, 0, 0, 0))
            rbit.Tag = 0
        End If
    End Sub

    ' Constructor
    Public Sub New()
        Dim rct As Rectangle
        Dim idx As Integer

        InitializeComponent()

        commonSliderMode = False

        ' Load with default RGB values in the slider
        lblRed.Text = Convert.ToString(Convert.ToInt32(sldRed.Value), 16)
        lblBlue.Text = Convert.ToString(Convert.ToInt32(sldBlue.Value), 16)
        lblGreen.Text = Convert.ToString(Convert.ToInt32(sldGreen.Value), 16)
        lblPaintSteps.Text = Convert.ToString(Convert.ToInt32(sldPaintSteps.Value))

        ' Load default polynomial
        ' LFSR32: x^32 + x^22 + x^2 + x + 1
        For idx = 1 To 32
            rct = FindName("rbit" & Convert.ToString(idx))
            If idx = 1 Or idx = 2 Or idx = 22 Or idx = 32 Then
                rct.Tag = 0
            Else
                rct.Tag = 1
            End If
            FlipBitTag(rct)
        Next

        SupportedOrientations = SupportedPageOrientation.Portrait Or SupportedPageOrientation.Landscape

        ' Sample code to localize the ApplicationBar
        'BuildLocalizedApplicationBar()

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

    Private Sub btnPaint_Click(sender As Object, e As RoutedEventArgs) Handles btnPaint.Click
        Dim uriParams As String
        Dim rct As Rectangle
        Dim idx As Integer

        If commonSliderMode = True Then
            uriParams = "red=" & Convert.ToInt32(sldRed.Value) & "&" & _
                       "green=" & Convert.ToInt32(sldRed.Value) & "&" & _
                       "blue=" & Convert.ToInt32(sldRed.Value) & "&" & _
                       "steps=" & Convert.ToInt32(sldPaintSteps.Value)
        Else
            uriParams = "red=" & Convert.ToInt32(sldRed.Value) & "&" & _
                       "green=" & Convert.ToInt32(sldGreen.Value) & "&" & _
                       "blue=" & Convert.ToInt32(sldBlue.Value) & "&" & _
                       "steps=" & Convert.ToInt32(sldPaintSteps.Value)
        End If
        uriParams = uriParams & "&" & "poly=0"
        For idx = 1 To 32
            rct = FindName("rbit" & idx)
            If Convert.ToInt32(rct.Tag) = 1 Then
                uriParams = uriParams & "," & Convert.ToString(idx)
            End If
        Next

        NavigationService.Navigate(New Uri("/ImageCanvas.xaml?" & uriParams, UriKind.Relative))
    End Sub

    Private Sub ToggleSliderMode()
        If commonSliderMode = True Then
            commonSliderMode = False
            sldBlue.IsEnabled = True
            sldGreen.IsEnabled = True
        Else
            commonSliderMode = True
            sldBlue.IsEnabled = False
            sldGreen.IsEnabled = False
        End If
    End Sub

    Private Sub sldBlue_Hold(sender As Object, e As Input.GestureEventArgs) Handles sldBlue.Hold
        ToggleSliderMode()
    End Sub

    Private Sub sldBlue_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldBlue.ValueChanged
        If Convert.ToInt32(e.NewValue) <> Convert.ToInt32(e.OldValue) Then
            lblBlue.Text = Convert.ToString(Convert.ToInt32(e.NewValue), 16)
        End If
    End Sub

    Private Sub sldGreen_Hold(sender As Object, e As Input.GestureEventArgs) Handles sldGreen.Hold
        ToggleSliderMode()
    End Sub

    Private Sub sldGreen_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldGreen.ValueChanged
        If Convert.ToInt32(e.NewValue) <> Convert.ToInt32(e.OldValue) Then
            lblGreen.Text = Convert.ToString(Convert.ToInt32(e.NewValue), 16)
        End If
    End Sub

    Private Sub sldRed_Hold(sender As Object, e As Input.GestureEventArgs) Handles sldRed.Hold
        ToggleSliderMode()
    End Sub

    Private Sub sldRed_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldRed.ValueChanged
        If Convert.ToInt32(e.NewValue) <> Convert.ToInt32(e.OldValue) Then
            lblRed.Text = Convert.ToString(Convert.ToInt32(e.NewValue), 16)
        End If
    End Sub

    Private Sub sldPaintSteps_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldPaintSteps.ValueChanged
        If Convert.ToInt32(e.NewValue) <> Convert.ToInt32(e.OldValue) Then
            lblPaintSteps.Text = Convert.ToString(Convert.ToInt32(e.NewValue))
        End If
    End Sub

    Private Sub rbit1_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit1.Tap
        FlipBitTag(rbit1)
    End Sub
    Private Sub rbit2_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit2.Tap
        FlipBitTag(rbit2)
    End Sub
    Private Sub rbit3_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit3.Tap
        FlipBitTag(rbit3)
    End Sub
    Private Sub rbit4_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit4.Tap
        FlipBitTag(rbit4)
    End Sub
    Private Sub rbit5_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit5.Tap
        FlipBitTag(rbit5)
    End Sub
    Private Sub rbit6_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit6.Tap
        FlipBitTag(rbit6)
    End Sub
    Private Sub rbit7_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit7.Tap
        FlipBitTag(rbit7)
    End Sub
    Private Sub rbit8_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit8.Tap
        FlipBitTag(rbit8)
    End Sub
    Private Sub rbit9_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit9.Tap
        FlipBitTag(rbit9)
    End Sub
    Private Sub rbit10_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit10.Tap
        FlipBitTag(rbit10)
    End Sub
    Private Sub rbit11_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit11.Tap
        FlipBitTag(rbit11)
    End Sub
    Private Sub rbit12_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit12.Tap
        FlipBitTag(rbit12)
    End Sub
    Private Sub rbit13_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit13.Tap
        FlipBitTag(rbit13)
    End Sub
    Private Sub rbit14_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit14.Tap
        FlipBitTag(rbit14)
    End Sub
    Private Sub rbit15_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit15.Tap
        FlipBitTag(rbit15)
    End Sub
    Private Sub rbit16_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit16.Tap
        FlipBitTag(rbit16)
    End Sub
    Private Sub rbit17_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit17.Tap
        FlipBitTag(rbit17)
    End Sub
    Private Sub rbit18_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit18.Tap
        FlipBitTag(rbit18)
    End Sub
    Private Sub rbit19_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit19.Tap
        FlipBitTag(rbit19)
    End Sub
    Private Sub rbit20_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit20.Tap
        FlipBitTag(rbit20)
    End Sub
    Private Sub rbit21_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit21.Tap
        FlipBitTag(rbit21)
    End Sub
    Private Sub rbit22_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit22.Tap
        FlipBitTag(rbit22)
    End Sub
    Private Sub rbit23_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit23.Tap
        FlipBitTag(rbit23)
    End Sub
    Private Sub rbit24_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit24.Tap
        FlipBitTag(rbit24)
    End Sub
    Private Sub rbit25_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit25.Tap
        FlipBitTag(rbit25)
    End Sub
    Private Sub rbit26_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit26.Tap
        FlipBitTag(rbit26)
    End Sub
    Private Sub rbit27_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit27.Tap
        FlipBitTag(rbit27)
    End Sub
    Private Sub rbit28_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit28.Tap
        FlipBitTag(rbit28)
    End Sub
    Private Sub rbit29_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit29.Tap
        FlipBitTag(rbit29)
    End Sub
    Private Sub rbit30_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit30.Tap
        FlipBitTag(rbit30)
    End Sub
    Private Sub rbit31_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit31.Tap
        FlipBitTag(rbit31)
    End Sub
    Private Sub rbit32_Tap(sender As Object, e As Input.GestureEventArgs) Handles rbit32.Tap
        FlipBitTag(rbit32)
    End Sub
End Class