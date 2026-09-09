# Plugin.WinUI.MVVMExpress.Pagination

Paging and search debounce for **MVVMExpress** lists.

[![NuGet](https://img.shields.io/nuget/v/Plugin.WinUI.MVVMExpress.Pagination.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Pagination)

```csharp
var page = new DelegatePagedCollection<Product>(
  (skip, take, ct) => catalog.ListAsync(skip, take, ct),
  pageSize: 20);

await page.RefreshAsync();
await page.LoadMoreAsync();
```

`PagedCollection<T>` uses `ObservableRangeCollection<T>` so each page is one collection reset. For a live inbox use `SnapshotCollection<T>` (load once, no threshold). `SearchQuery.Text` binds to an `Entry`; filter from `CommittedText`. Do not two-way bind Android `SearchBar`.

## Install

```bash
dotnet add package Plugin.WinUI.MVVMExpress.Pagination
```

Target framework: `net10.0`. Depends on [Core](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Core). Version `1.0.0`.

## Related

Product docs: [repository README](https://github.com/nuvyntralabs/Plugin.WinUI.MVVMExpress). License: MIT. Niladri Padhy / MauiEssentials.
