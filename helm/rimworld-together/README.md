### This helm chart allow you to deploy a Rimworld Together server on your kubernetes cluster.

To make the chart work, create a `values.secret.yaml` file and copy the structure of the `values.secret.exemple.yaml` file into it, filling the empty fields.

When installing / upgrading the chart, mind adding both `values.yaml` and `values.secret.yaml` with the `-f` flag to use both files.

⚠️ It doesn't handle scaling and pod concurrency, only 1 pod is supported, you'll have to scale verticaly if your server gets slow

⚠️ It only work with a Treafik loadbalancer installed upfront. To make it work you'll need to setup the following entrypoint:

```yaml
rimworld:
  expose:
    default: true
  exposedPort: 25555
  forwardedHeaders:
    insecure: false
    trustedIPs: []
  port: 25555
  protocol: TCP
  proxyProtocol:
    insecure: false
    trustedIPs: []
```

This should be added on your Treafik values, under the `ports` property. 