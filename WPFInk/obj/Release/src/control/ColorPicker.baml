   M S B A M L   `   `   `  ���� 
  WPFInk    WPFInk.ColorPicker   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     >�Name$  UserControl��$	��120��$	��150��.���� -    ��Name$ RootControl��$	��150��$	��120�����	#FF6B6A6A���	#FF434343$��0q� �� CornerRadius$ 8,8,8,8��._�� -    �Name$ 
LayoutRoot��$
��Auto��$
��Auto��$��0q� � RowDefinitions �� -    ��Name$ colorRow��$��* �$��50���� $
��Auto �$��0�� � ColumnDefinitions �� -    ��Name$ colorColumn��$��* �$��50���� -   $ rightColumn��$
��Auto �$��0��.+�� $��1��$��1��$��0,0,0,0q� �� $��0.85* �$	��100���� $��0.15* �$��60��$��10�� �� $��0.851* �$	��100���� $��0.149* �$��60��$��20���� $��0* ��� $��0* �.+��� -    ��Name$ BackgroundCanvas��$��Stretch=�$��1,1,1,1q�$��Stretch=����	#FFFF0000$��1��$��1��$��0���� $��1,1,1,1q����� 	 �� EndPoint$	 1,0.5&�
 �� 
StartPoint$
 0,0.5&�.���  � Color$ 	#00FFFFFF�� � Offset$ 1Q�� $ 	#FFFFFFFF��$ 0Q��� -   $ ColorCanvas��$��0,1,1,1q�$��99��$��125.5������ $	 0.5,1&�$
 0.5,0&�.��� $ 	#FF000000��$ 1Q�� $ 	#00000000��$ 0Q�.+�� -	   $ 
FinalColor��$��12��$��12��
 �� Top$ 119.333�� �� Left$ 92.997��.+�A� ���	#FFFFFFFF$��3��A� ���	#FF000000$��1,1,1,1q��� -
   $ resultCanvas��$
��18.5��$��Stretch=�$��2,2,2,2q�$��Stretch=�$��4�����	#FF000000$��1,1,1,1q�$ 6,6,6,6��$��1��
���   �$	��116���� -   $ RainbowBorder��$��1,1,1,1q�$��1��$��1,1,1,1q�$ 6,6,6,6��$��3��$��18�����	#FF262626$��125.5������ $	 0.5,1&�$
 0.5,0&�.��� $ 	#FFFF0000��� $ 	#FFFFFF00��$ 0.167Q�� $ 	#FF00FF00��$ 0.333Q�� $ 	#FF00FFFF��$	 0.5Q�� $ 	#FF0000FF��$ 0.667Q�� $ 	#FEFF00FF��$ 0.833Q�� $ 	#FFFF0000��$ 1Q�._��� -   $ 	HueCanvas��$��16��.+�� -   $ RainbowHandle��$��0,0,0,0q�$��10�����	#00FFFFFF$��Stretch=�$��12��$ -0.665��$ -0.329�� �� $
��0.5* ��� $��0* ��� $
��0.5* �.+�F� $��Stretch=�$��1,2,0,2q�$��Stretch=����	#FF000000 �� Stretch$
 Fill=����	#FF000000"����        �@KL @KL �    ��� 	 �� StrokeStartLineCap$ Round=� �� StrokeEndLineCap$ Round=� �� StrokeDashCap$ Round=� �� StrokeLineJoin$ Round=�F� $��Stretch=�$��0,2,1,2q�$��Stretch=�$��2�����	#FF000000$
 Fill=����	#FF000000"���� -1    ���� @KL � -1��� 	$ Round=�$ Round=��� -    ��Name$ CompactPanel��$��Stretch=�$��0,0,0,0q�$��Stretch=�$
��Auto�� �� Orientation$ Vertical=�$��0��$��1��$��1��$y�	Collapsed=�.+��� -    ��Name$ CompactRGBText��$��255,255,255�����	#FFFFFFFF$
��Auto�����	#FF333333$
��Auto��$��2,2,2,2q��� -   $ CompactHexText�����	#FFFFFFFF$
��Auto�����	#FF333333$
��Auto��$��2,2,2,2q�