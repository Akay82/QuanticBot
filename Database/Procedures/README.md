# PostgreSQL Procedures

Apply the procedure scripts after the EF migration:

```powershell
psql -U postgres -d Quantic -f Database/Procedures/001_order_workflow.sql
```

API procedure usage:

| API route | PostgreSQL procedure |
| --- | --- |
| `POST /api/orders/{id}/cancel` | `quantic.cancel_order(bigint)` |
| `POST /api/orders/{id}/execute` | `quantic.execute_order(bigint, numeric)` |

Standard CRUD endpoints use EF Core parameterized queries. Procedures are reserved for multi-table transactional workflows.
