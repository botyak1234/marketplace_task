namespace TaskMarketplace.Contracts.Enums;

/// <summary>
/// Статусы задач в системе
/// </summary>
public enum MarketplaceTaskStatus
{
    /// <summary>
    /// Новая задача, доступная для взятия
    /// </summary>
    New,
    
    /// <summary>
    /// Задача взята пользователем в работу
    /// </summary>
    Taken,
    
    /// <summary>
    /// Задача выполнена и отправлена на проверку
    /// </summary>
    Submitted,
    
    /// <summary>
    /// Задача одобрена администратором
    /// </summary>
    Approved,
    
    /// <summary>
    /// Задача отклонена администратором
    /// </summary>
    Rejected
}