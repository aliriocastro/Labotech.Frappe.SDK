using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Labotech.Frappe.Connector.Contracts;
using Labotech.Frappe.Connector.Core;
using Labotech.Frappe.Connector.Services;
using Labotech.Frappe.Core;
using Xunit;

namespace Labotech.Frappe.Connector.Tests;

public class FrappeQueryFluentTests
{
    private const string Doctype = "Customer";

    private static FrappeQueryFluent<TestEntity> NewQuery(IFrappeService? service = null)
        => new FrappeQueryFluent<TestEntity>(Doctype, service ?? new RecordingFrappeService());

    // ---------- OrderByAsString (regression for the "single orderby silently dropped" bug) ----------

    [Fact]
    public void OrderByAsString_NoEntries_ReturnsEmpty()
    {
        var sut = NewQuery();

        Assert.Equal(string.Empty, sut.OrderByAsString());
    }

    [Fact]
    public void OrderByAsString_SingleEntry_IncludesIt_RegressionForCountGreaterThanOneBug()
    {
        var sut = NewQuery();
        sut.AddOrderBy("name");

        // Before the fix this returned string.Empty because the impl used `Count > 1`.
        Assert.Equal("name ASC", sut.OrderByAsString());
    }

    [Fact]
    public void OrderByAsString_TwoEntries_JoinsWithComma()
    {
        var sut = NewQuery();
        sut.AddOrderBy("name");
        sut.AddOrderBy("modified", asc: false);

        Assert.Equal("name ASC, modified DESC", sut.OrderByAsString());
    }

    // ---------- FieldsAsString (regression for mutating Fields list bug) ----------

    [Fact]
    public async Task FieldsAsString_DoesNotMutateFieldsAcrossInvocations()
    {
        var service = new RecordingFrappeService();
        var sut = NewQuery(service);
        sut.WithFields("custom");
        var initialCount = sut.Fields.Count;

        await sut.FetchAsync();
        await sut.FetchAsync();
        await sut.FetchAsync();

        // Before the fix, FieldsAsString called Fields.AddRange(_defaultFields) on every call.
        Assert.Equal(initialCount, sut.Fields.Count);
        Assert.Equal(3, service.GetListInvocations);
    }

    [Fact]
    public void FieldsAsString_WithAllFields_SerializesStarOnly()
    {
        var sut = NewQuery();
        sut.WithAllFields();

        var json = sut.FieldsAsString();

        Assert.Contains("\"*\"", json);
    }

    [Fact]
    public void FieldsAsString_OnlyWithFields_DoesNotIncludeDefaults()
    {
        var sut = NewQuery();
        sut.OnlyWithFields("custom_a", "custom_b");

        var json = sut.FieldsAsString();

        Assert.Contains("custom_a", json);
        Assert.Contains("custom_b", json);
        Assert.DoesNotContain("modified_by", json);
        Assert.DoesNotContain("creation", json);
    }

    [Fact]
    public void FieldsAsString_WithFields_IncludesDefaults()
    {
        var sut = NewQuery();
        sut.WithFields("custom_a");

        var json = sut.FieldsAsString();

        Assert.Contains("custom_a", json);
        Assert.Contains("name", json);
        Assert.Contains("modified_by", json);
        Assert.Contains("creation", json);
    }

    // ---------- ConditionsAsString ----------

    [Fact]
    public void ConditionsAsString_NoConditions_ReturnsEmpty()
    {
        var sut = NewQuery();

        Assert.Equal(string.Empty, sut.ConditionsAsString());
    }

    [Fact]
    public void ConditionsAsString_EqualToCondition_SerializesAsTriple()
    {
        var sut = NewQuery();
        sut.AddCondition("status", "Active");

        var json = sut.ConditionsAsString();

        Assert.Contains("\"status\"", json);
        Assert.Contains("\"=\"", json);
        Assert.Contains("\"Active\"", json);
    }

    [Fact]
    public void ConditionsAsString_InCondition_WithMultipleValues_SerializesArray()
    {
        var sut = NewQuery();
        sut.AddInCondition("status", "Active", "Paused");

        var json = sut.ConditionsAsString();

        Assert.Contains("\"in\"", json);
        Assert.Contains("\"Active\"", json);
        Assert.Contains("\"Paused\"", json);
    }

    [Fact]
    public void ConditionsAsString_InCondition_WithSingleValue_TreatsAsScalar()
    {
        var sut = NewQuery();
        sut.AddInCondition("status", "Active");

        var json = sut.ConditionsAsString();

        Assert.Contains("\"in\"", json);
        Assert.Contains("\"Active\"", json);
        // The single-value array is unwrapped to a scalar by impl design.
        Assert.DoesNotContain("[\"Active\"]", json);
    }

    [Fact]
    public void ConditionsAsString_BetweenCondition_SerializesPair()
    {
        var sut = NewQuery();
        sut.AddBetweenCondition("date", "2024-01-01", "2024-12-31");

        var json = sut.ConditionsAsString();

        Assert.Contains("2024-01-01", json);
        Assert.Contains("2024-12-31", json);
    }

    [Fact]
    public void AddCondition_WithBetweenOperator_Throws()
    {
        var sut = NewQuery();

        Assert.Throws<ArgumentException>(() =>
            sut.AddCondition("date", FrappeFilterOperator.Between, "2024-01-01"));
    }

    [Fact]
    public void AddCondition_WithInOperator_Throws()
    {
        var sut = NewQuery();

        Assert.Throws<ArgumentException>(() =>
            sut.AddCondition("status", FrappeFilterOperator.In, "Active"));
    }

    [Fact]
    public void AddCondition_WithNotInOperator_Throws()
    {
        var sut = NewQuery();

        Assert.Throws<ArgumentException>(() =>
            sut.AddCondition("status", FrappeFilterOperator.NotIn, "Active"));
    }

    // ---------- Take / Skip / AllRows propagation ----------

    [Fact]
    public async Task FetchAsync_PropagatesTakeSkipAndOrderToService()
    {
        var service = new RecordingFrappeService();
        var sut = NewQuery(service);
        sut.Take(50).Skip(10).AddOrderBy("name");

        await sut.FetchAsync();

        Assert.Equal(Doctype, service.LastDocType);
        Assert.Equal("name ASC", service.LastOrderBy);
        Assert.Equal(10, service.LastLimitStart);
        Assert.Equal(50, service.LastLimitPageLength);
    }

    [Fact]
    public async Task FetchAsync_AllRows_SendsLimitPageLengthZero()
    {
        var service = new RecordingFrappeService();
        var sut = NewQuery(service);
        sut.AllRows();

        await sut.FetchAsync();

        Assert.Equal(0, service.LastLimitPageLength);
    }

    [Fact]
    public async Task FetchAsync_PassesCancellationTokenThrough()
    {
        var service = new RecordingFrappeService();
        using var cts = new CancellationTokenSource();
        var sut = NewQuery(service);

        await sut.FetchAsync(cts.Token);

        Assert.Equal(cts.Token, service.LastCancellationToken);
    }

    // ---------- Test fixtures ----------

    private sealed class TestEntity : IFrappeEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Doctype { get; set; } = "Customer";
        public int? Idx { get; set; }
        public int? DocStatus { get; set; }
        public DateTime Creation { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime Modified { get; set; }
        public string Owner { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hand-rolled fake recording the args passed to GetListAsync. All other
    /// IFrappeService members throw NotImplementedException because the
    /// fluent query under test only exercises GetListAsync.
    /// </summary>
    private class RecordingFrappeService : IFrappeService
    {
        public string? LastDocType;
        public string? LastFields;
        public string? LastFilters;
        public string? LastParent;
        public string? LastOrderBy;
        public int LastLimitStart;
        public int LastLimitPageLength;
        public CancellationToken LastCancellationToken;
        public int GetListInvocations;

        public virtual Task<IEnumerable<TEntity>> GetListAsync<TEntity>(
            string docType, string fields, string filters, string parent,
            string orderBy, int limitStart, int limitPageLength,
            CancellationToken cancellationToken = default) where TEntity : IFrappeBaseEntity
        {
            GetListInvocations++;
            LastDocType = docType;
            LastFields = fields;
            LastFilters = filters;
            LastParent = parent;
            LastOrderBy = orderBy;
            LastLimitStart = limitStart;
            LastLimitPageLength = limitPageLength;
            LastCancellationToken = cancellationToken;
            return Task.FromResult<IEnumerable<TEntity>>(Array.Empty<TEntity>());
        }

        // Everything else is unused by these tests.
        public Task<int> GetCountAsync(string docType, string filters, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<T> GetDocByFilterAsync<T>(string docType, string filters, string parent, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<T> GetDocByNameAsync<T>(string docType, string docName, string parent, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<TField> GetFieldValueAsync<TField>(string docType, string fieldName, string filters, string parent, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<TEntity> GetResourceAsync<TEntity>(string docType, string docName, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<string> GetResourceAsRawAsync(string docType, string docName, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IEnumerable<TEntity>> GetResourcesAsync<TEntity>(string docType, string fields, string filters, string parent, string orderBy, int limitStart, int limitPageLength, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<TEntity> PostResourceAsync<TEntity>(TEntity content, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<T> PostResourceAsync<T>(T entity, string docType, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteResourceAsync(string docType, string docName, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteResourceAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<TEntity> PutResourceAsync<TEntity>(TEntity content, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<TEntity> PutResourceAsync<TEntity>(TEntity entity, string docType, string name, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<TResult> GetSingleValueAsync<TResult>(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<string> RenameDocAsync(string docType, string oldName, string newName, bool merge, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<TEntity> SaveDocAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task DeleteDocAsync(string docType, string docName, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> HasPermissionAsync(string docType, string docName, FrappeDocPermission permType, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<TEntity> InsertAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<IEnumerable<string>> InsertManyAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<TEntity> UpdateAsync<TEntity>(string docType, string docName, TEntity entity, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<TEntity> UpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task<IDictionary<dynamic, string>> BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entitiesToUpdate, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
        public Task DeferredInsertAsync<TEntity>(IEnumerable<TEntity> entitiesToInsert, string docType, CancellationToken ct = default) where TEntity : IFrappeBaseEntity => throw new NotImplementedException();
    }
}
