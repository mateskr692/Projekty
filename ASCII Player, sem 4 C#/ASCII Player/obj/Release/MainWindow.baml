   M S B A M L   `   `   `  ���� G  CASCII Player, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:ASCIIPlayerASCIIPlayer      ASCIIPlayer.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     +localclr-namespace:ASCIIPlayer       � Title$  
MainWindow��$	��800��$	��600��.����  �� Padding$ 10q�._��  � ColumnDefinitions �� $��* ��� $
��auto �.+��� $	��300��$	��300��$��	0,0,0,0.4q�����  �� Path$ ASCII_Image� �� Mode$ OneWay=��� -    ��Name$ TabControl_Options�� �� TabStripPlacement$ Right=�$��20,0,-0.4,0.4q�$��Right=�$��1��$	��300������  �� 
TargetType" ��.���  �� Property �@�LayoutTransform	 �� Value	 �� 
 �� Angle$
 90Q�.@��� $��General��.���� $ 10q�._��� .+��� $��Open: ��$��0,0,0,5q�� $��0,0,0,20q� �� $��* ��� $��5 ��� $
��auto �.+��� -    ��Name$ TextBox_Input��$��0��$	��100������ $ Input� �� UpdateSourceTrigger$ PropertyChanged=��� -    ��Name$ Button_openFile��$��2��$��Browse���� -    ��Name$ CheckBox_Color�� d� 	IsChecked �� $ UseColor�.��	Use Color�� -   $��Border_UseColor��{�.  ._��� $��	20,0,0,20q�.+��� $��Color Render Mode:��$��0,0,0,5q�	�  	� 	GroupName$ 
ColorSpeed��$ FalseY�.��Slow	� $ 
ColorSpeed�� �� $ 	FastColor�.��Fast�� -   $��
SaveOutput��$��0,0,0,5q�.��Save Video Output�� -   $��Border_Save��{�.  ._�� $��0,0,0,20q� �� $��* ��� $��5 ��� $
��auto �.+��� -   $ TextBox_Output��$	��100������ $ Output�$ PropertyChanged=��� -	   $ Button_SaveFile��$��2��$��Browse���  � RowDefinitions �� �� ��  �� $
��auto ��� $��10 ��� $
��auto �.+��� -
   $ Button_Convert��$��0q�$��Render Video��$	��3,1q�$	��100���� -   $ Button_Pause��$��2��$��Pause��$	��3,1q��� -   $ Button_SaveImage��$��1��$��0,10,0,0q�$��
Save Image��$	��100���� -   $ Button_CopyText��$��2��$��0,10,0,0q�$��	Copy Text��$	��100���� $
��Font��$��6,2,10,2q�.���� $ 10q�._��� .+�� $��0,0,0,10q�$	��120�� �� $
��auto ��� $��20 ��� $
��auto �.+��� $��Font:���� $��2��.+��� -   $��FontName������ $ FontName�$ OneWay=��� -   $��	FontStyle������ $ 	FontStyle�$ OneWay=��� -   $��FontSize������ $ FontSize�$ OneWay=��� -   $ Button_ChooseFont��$��Change Font��$��1q�$
��Left=�$��0,0,0,20q�� $��0,0,0,20q� �� $
��auto ��� $��* �.+��� $��
Characters��$��0,0,5,0q��� -   $��TextBox_FontCharacters��$��1������ $ FontCharacters�$ PropertyChanged=�� $��0,0,0,20q� �� $
��auto ��� $��* ��� $
��auto � �� $
��auto ��� $��10 ��� $
��auto �.+��� $��Foreground Color���� -   $��Rectangle_ColorForeground��$��1��$
��10,0q�
���   ����� $ ForegroundBrush�$ OneWay=��� -   $��Button_ColorForeground��$��2��.��Change�� $��Background Color��$��2���� -   $��Rectangle_ColorBackground��$��1��$
��10,0q�
���   �$��2������ $ BackgroundBrush�$ OneWay=��� -   $��Button_ColorBackground��$��2��$��2��.��Change�� -   $��Image_FontPreview��$
��Left=�$��0,0,0,20q�$	��Top=�$
��None=����� $ FontPreview�$ OneWay=��� $��	Splitting��.���� $ 10q�._��� .+�� $��0,0,0,20q�$	��120�� �� $��* ��� $��10 ��� $
��auto �.+��� $��	Splitter:���� -   $��ComboBox_Splitter��$��2��$	��120��.@��� -    ��Name$" ComboBoxItem_SplitterRegular��$��Regular���� -   $ ComboBoxItem_SplitterEdge��$��Edge Detection��� $��20,5,0,0q� �� $
��auto ��� $��10 ��� $��* � �� $
��auto ��� $��5 ��� $
��auto �.+��� $��Width: ���� -   $��TextBox_SplitterWidth��$��2������ $ SplitterWidth�$ PropertyChanged=��� $��Height: ��$��2���� -   $��TextBox_SplitterHeight��$��2��$��2������ $ SplitterHeight�$ PropertyChanged=��� $��
Comparator��.���� $ 10q�._��� .+�� $��0,0,0,20q�$	��120�� �� $��* ��� $��10 ��� $
��auto �.+��� $��Comparator:���� -   $��ComboBox_Comparator��$��2��$	��100��$��0q�.@��� -   $(��"ComboBoxItem_ComparatorCentralized��$��Centralized���� -   $%��ComboBoxItem_ComparatorCellular��$��Cellular���� -    $%��ComboBoxItem_ComparatorGaussian��$��Gaussian���� -!   $&�� ComboBoxItem_ComparatorGrayscale��$��	Grayscale���� -"   $"��ComboBoxItem_ComparatorExact��$��Exact���� -#   $"��Border_ComparatorCentralized��$y�Visible=�._��� .+�� $��20,5,0,0q� �� $
��auto ��� $��10 ��� $��* � �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto �.+��� $��Left / Right Margin��$��0,0,0,5q��� -$   $ ��CentralizedLeftRightMargin��$	��150�� � Minimum$
 0.05Q� � Maximum$
 0.45Q� �� TickFrequency$
 0.05Q� � SmallChange$
 0.05Q� � LargeChange$
 0.05Q� �� TickPlacement$ BottomRight=� �� AutoToolTipPlacement$ BottomRight=� �� AutoToolTipPrecision$ 2�� �� IsSnapToTickEnabled . $��1�� � Value �� $ Centralized_LRMargin�$ PropertyChanged=��� $��Up / Down Margin��$��2��$��0,0,0,5q��� -%   $��CentralizedUpDownMargin��$	��150��$
 0.05Q�$
 0.45Q�$
 0.05Q�$
 0.05Q�$
 0.05Q�$ BottomRight=�$ BottomRight=�$ 2�� . $��3�� �� $ Centralized_UDMargin�$ PropertyChanged=��� $��Central Weight��$��4��$��0,20,0,0q��� -&   $��CentralizedCenterWeight��$��4��$��2��$��0,20,0,0q�$
��Left=�$��50������ $ Centralized_CenterWeight�$ PropertyChanged=��� $��Left / Right Weight��$��5��$��0,10,0,0q��� -'   $ ��CentralizedLeftRightWeight��$��5��$��2��$��0,10,0,0q�$
��Left=�$��50������ $ Centralized_LRWeight�$ PropertyChanged=��� $��Up / Down Weight��$��6��$��0,10,0,0q��� -(   $��CentralizedUpDownWeight��$��6��$��2��$��0,10,0,0q�$
��Left=�$��50������ $ Centralized_UDWeight�$ PropertyChanged=��� -)   $��Border_ComparatorCellular��$y�	Collapsed=�._��� .+�� $��20,5,0,0q�$
��Left=� �� $
��auto ��� $��10 ��� $��* � �� $
��auto ��� $
��auto �.+��� $��X-Dimension���� -*   $��CellularWidth��$��2��$��50������ $ 
Cellular_X�$ PropertyChanged=��� $��Y-Dimension��$��1��$��0,5,0,0q��� -+   $��CellularHeight��$��2��$��50��$��1��$��0,5,0,0q����� $ 
Cellular_Y�$ PropertyChanged=�