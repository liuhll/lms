---
title: 名词解释
lang: zh-cn
---

## 主机

**主机**是封装应用资源的对象,用于托管应用和管理应用的生命周期。silky框架使用了.net的两种类型的主机:

1. 用于托管业务服务应用的[通用主机](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/generic-host),对于该类型的主机,不提供http服务(不会对外部公布Http端口)。由于业务服务应用之间主要通信是基于dotnetty实现的rpc通信框架(底层使用的通信协议是`TCP`协议),对于普通业务应用服务来说,托管该类型的主机并不需要提供http服务。

2. 用于托管网关服务应用的[Web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0),网关本身不实现具体的业务,通过依赖其他微服务应用的应用接口层(包)，在http请求到达后,通过`webapi路径`+`http请求方法`在路由表中找到相关的服务路由信息,并通过负载均衡算法,找到相应的服务提供者,并通过rpc通信与相应的服务提供者实例进行通信,并将返回结果封装后返回给调用者(前端)。开发者可以在网关项目引用或是自定义相应的[Asp.net Core 中间件](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0)

## 路由

路由是指通过`webapi路径`+`http请求方法`或是通过`ServiceId`在路由表查找相应的服务条目信息的过程。
  
- 在网关应用,查找服务条目信息是通过`webapi路径`+`http请求方法`。
- 在业务微服务应用中, 通过`ServiceId`查找服务条目信息。`ServiceId`是通过服务调用对应的方法的完全限定名和参数名组合生成的。

## 服务条目

每个业务应用服务都会将已经实现的业务服务接口方法生成一个对应的服务条目,每个服务条目都会根据方法的权限限定名和参数名生成唯一的服务Id(`ServiceId`)。在应用服务启动时,每个服务实例都会想服务注册中心注册或更新服务条目对应的服务提供者的地址信息。

## 路由表

由服务条目组成的集合。微服务集群的路由表会根据服务实例的启动(注册)和停止(注销)得到更新。

在微服务应用实例启动时,会自动注册或更新服务注册中心的该服务条目的地址信息。同时,订阅了服务条目的其他微服务应用也会在内存中更新相应的路由表。

## 服务注册中心

主要用于保存微服务集群的路由表信息。会根据服务提供者实例的启动(注册)和停止(注销)更新服务条目的地址列表。

## RPC

RPC 全称 Remote Procedure Call——远程过程调用。是为了解决远程调用服务的一种技术，使得调用者像调用本地服务一样方便透明。简单的说，RPC就是从一台机器（客户端）上通过参数传递的方式调用另一台机器（服务器）上的一个函数或方法（可以统称为服务）并得到返回的结果。

## 服务提供者

在rpc通信过程中,负责提供服务接口定义和服务实现类，在rpc通信过程中,作为服务端的一方。

## 服务消费者/服务调用者

在rpc通信过程中,提供应用服务接口动态生成本地代理,并使用代理通过网络请求,与服务提供者进行通信，并将结果返回给调用者的的一方。

::: warning

对于一个业务微服务应用而言，既可以作为服务提供者,也可能是服务消费者,主要看在一个rpc通信过程中,是作为提供服务的一方,还是作为调用服务的一方。

:::

## 服务治理

服务治理是主要针对分布式服务框架，微服务，处理服务调用之间的关系，服务发布和发现（谁是提供者，谁是消费者，要注册到哪里），出了故障谁调用谁，服务的参数都有哪些约束,如何保证服务的质量？如何服务降级和熔断？怎么让服务受到监控，提高机器的利用率?

::: tip

当前silky框架暂未完全实现服务治理的所有要求,后期开发过程中将不断完善

:::

## 缓存拦截

在rpc通信过程中,为减少网络请求,提供分布式应用的性能,如果数据以及得到缓存,则直接从缓存服务中获取相应的数据,并返回给服务调用者。