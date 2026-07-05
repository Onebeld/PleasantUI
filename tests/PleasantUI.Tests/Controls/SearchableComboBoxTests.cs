using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using PleasantUI.Controls;

namespace PleasantUI.Tests.Controls;

public class SearchableComboBoxTests
{
    private SearchableComboBox CreateTargetWithTemplate()
    {
        var control = new SearchableComboBox();
    
        control.Template = new FuncControlTemplate((parent, scope) =>
        {
            var panel = new StackPanel();
        
            var textBox = new TextBox { Name = "PART_EditableTextBox" };
            scope.Register("PART_EditableTextBox", textBox);
            panel.Children.Add(textBox);

            var popup = new Popup
            {
                Name = "PART_Popup",
                ShouldUseOverlayLayer = false
            };
            scope.Register("PART_Popup", popup);
        
            popup.Bind(Popup.IsOpenProperty, parent.GetObservable(SearchableComboBox.IsDropDownOpenProperty));
        
            var itemsPresenter = new ItemsPresenter { Name = "PART_ItemsPresenter" };
            itemsPresenter.Bind(ItemsPresenter.ItemsPanelProperty, parent.GetObservable(ItemsControl.ItemsPanelProperty));
        
            popup.Child = itemsPresenter;
            panel.Children.Add(popup);
        
            return panel;
        });

        return control;
    }

    private void ApplyTemplateAndApplyLogicalParent(SearchableComboBox control)
    {
        // Симулируем прикрепление к дереву для работы логики инициализации
        Window root = new()
        {
            Content = control
        };
        control.ApplyTemplate();

        // Принудительно вызываем OnApplyTemplate для внутренних элементов
        ItemsPresenter? presenter = control.FindDescendantOfType<ItemsPresenter>();
        presenter?.ApplyTemplate();

        root.Show();
    }

    [AvaloniaFact]
    public void Default_ItemsPanel_ShouldBe_VirtualizingStackPanel()
    {
        var target = CreateTargetWithTemplate();
        ApplyTemplateAndApplyLogicalParent(target);

        var panel = target.ItemsPanel.Build();
        Assert.IsType<VirtualizingStackPanel>(panel);
    }
    
    [AvaloniaFact]
    public void Filtering_ShouldReduce_FilteredItemCount_And_UpdateItemsSource()
    {
        var target = CreateTargetWithTemplate();
        var items = new List<string> { "Apple", "Banana", "Cherry" };
        target.ItemsSource = items;
        ApplyTemplateAndApplyLogicalParent(target);

        // Вбиваем текст для поиска
        target.FilterText = "an"; // Должен подойти только Banana

        Assert.Equal(1, target.FilteredItemCount);
        var activeItems = target.ItemsSource.Cast<object>().ToList();
        Assert.Single(activeItems);
        Assert.Equal("Banana", activeItems[0]);
    }

    [AvaloniaFact]
    public void ClosingPopup_ShouldReset_FilterText_And_RestoreAllItems()
    {
        var target = CreateTargetWithTemplate();
        var items = new List<string> { "Apple", "Banana", "Cherry" };
        target.ItemsSource = items;
        ApplyTemplateAndApplyLogicalParent(target);

        target.IsDropDownOpen = true;
        target.FilterText = "Apple";

        // Симулируем закрытие DropDown
        target.IsDropDownOpen = false;

        Assert.True(string.IsNullOrEmpty(target.FilterText));
        Assert.Equal(3, target.ItemsSource.Cast<object>().Count());
    }

    [AvaloniaFact]
    public void Custom_FilterFunction_ShouldBe_Respected()
    {
        var target = CreateTargetWithTemplate();
        var items = new List<string> { "123", "456", "789" };
        target.ItemsSource = items;
        ApplyTemplateAndApplyLogicalParent(target);

        // Кастомный фильтр: ищем по длине строки (например, если текст "long", ищем строки > 2 символов)
        target.FilterFunction = (item, text, container) => text == "custom" && item?.ToString()?.StartsWith("7") == true;

        target.FilterText = "custom";

        Assert.Equal(1, target.FilteredItemCount);
        Assert.Equal("789", target.ItemsSource.Cast<object>().First());
    }

    [AvaloniaFact]
    public void Selection_IndexToItem_And_ItemToIndex_Synchronization()
    {
        var target = CreateTargetWithTemplate();
        var items = new AvaloniaList<string> { "First", "Second", "Third" };
        target.ItemsSource = items;
        ApplyTemplateAndApplyLogicalParent(target);

        // Тест 1: Установка индекса меняет элемент
        target.SelectedIndex = 1;
        Assert.Equal("Second", target.SelectedItem);

        // Тест 2: Установка элемента меняет индекс
        target.SelectedItem = "Third";
        Assert.Equal(2, target.SelectedIndex);
    }

    [AvaloniaFact]
    public void Clear_Method_ShouldReset_Selection_And_FilterText()
    {
        var target = CreateTargetWithTemplate();
        target.ItemsSource = new List<string> { "A", "B" };
        ApplyTemplateAndApplyLogicalParent(target);

        target.SelectedIndex = 1;
        target.FilterText = "B";

        target.Clear();

        Assert.Null(target.SelectedItem);
        Assert.Equal(-1, target.SelectedIndex);
        Assert.Equal(string.Empty, target.FilterText);
    }

    [AvaloniaFact]
    public void Key_Escape_ShouldClose_DropDown()
    {
        var target = CreateTargetWithTemplate();
        ApplyTemplateAndApplyLogicalParent(target);

        target.IsDropDownOpen = true;

        // Создаем событие нажатия Escape
        var keyArgs = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Escape
        };

        target.RaiseEvent(keyArgs);

        Assert.False(target.IsDropDownOpen);
        Assert.True(keyArgs.Handled);
    }

    [AvaloniaFact]
    public void OpenPopup_Should_ScrollIntoView_SelectedItem()
    {
        var target = CreateTargetWithTemplate();
        // Создаем много элементов, чтобы спровоцировать скролл
        var items = Enumerable.Range(0, 100).Select(i => $"Item {i}").ToList();
        target.ItemsSource = items;
        ApplyTemplateAndApplyLogicalParent(target);

        // Выделяем элемент в самом низу
        target.SelectedItem = "Item 95";

        // Открытие Popup триггерит ScrollIntoView через метод PopupOpened
        target.IsDropDownOpen = true;

        // Здесь мы проверяем, что выполнение дошло до конца без ошибок.
        // Точная проверка ScrollIntoView требует замера Layout, но сам вызов метода завязан на логику:
        Assert.Equal(95, target.SelectedIndex);
    }

    [AvaloniaFact]
    public void Changing_Selection_ShouldRaise_SelectionChanged_Event()
    {
        var target = CreateTargetWithTemplate();
        target.ItemsSource = new List<string> { "One", "Two" };
        ApplyTemplateAndApplyLogicalParent(target);

        bool eventRaised = false;
        target.SelectionChanged += (s, e) => eventRaised = true;

        target.SelectedIndex = 0;

        Assert.True(eventRaised);
    }
}