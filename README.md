- Add claim

```bash
db.aspnetusers.updateOne( { _id: ObjectId('...') }, { $push: { claims: { _id: 0, userId: null, claimType: "isAdmin", claimValue: "true" } } })
```
