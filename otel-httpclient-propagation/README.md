Sample app to see how OpenTelemetry propagates Baggage via a HttpClient request.

Sample response content from request to /:
```
HTTP Headers:
Host=localhost:8042
baggage=OtelBag2=foofoo1
Correlation-Context=ActivityBag=foobar
traceparent=00-3a86d4f449374f024ae3ca77020cd10a-a9106744316b74a7-01

Activity Baggage:
OtelBag2=foofoo1

OTEL Baggage:
OtelBag2=foofoo1

Tags:
net.host.name=localhost
http.method=GET
http.scheme=http
http.target=/internal
http.url=http://localhost:8042/internal
http.flavor=1.1
```