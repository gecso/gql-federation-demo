# ABOUT

Это демо проекта, илюстрирующего способ использования Federation на базе Hot Chocolate Fusion

## TODO

Создаем 2 простых web приложения, экспортирующих GraphQL endpoint-ы

Project.Web - проект отвечает за управление сущностью Projects, предоставляя методы для получения и мутации проектов

Conversation.Web - пердоставляем методы управления Conversation и Messages в рамкаж диалога

BFF - сервис с Federation, который объединяет API Project.Web и Conversation.Web

## Requirements

- Используем последнюю версию Hot Chocolate для GraphQL
- Для каждого проекта мы заведем тестовый проект на xunit
- Используем Code First подход

## Models

```
//  Project
type Project {
    id: ID,
    name: String
    description: String
}

// Conversation
type Conversation {
    id: ID,
    projectId: String
    createdAt: Date
    topic: String
    messages: [Message]
}

type Message {
    id: ID,
    conversation:  Conversation
    createdAt: Date
    text: String
    author: String
}
```

## Fusion CLI workflow (без ручного SDL в BFF)

1. Запустить subgraph сервисы:
    - `Project.Web` на `http://localhost:5249/graphql`
    - `Conversation.Web` на `http://localhost:5102/graphql`
2. Сгенерировать gateway graph:
    - `./scripts/fusion-compose.sh`
3. BFF читает скомпозированный graph из файла:
    - `src/BFF/fusion-gateway.graphql`

### Быстрый запуск всех сервисов

- Запуск всего стека: `./scripts/run-all.sh`
- GraphQL endpoints:
    - Project: `http://localhost:5249/graphql`
    - Conversation: `http://localhost:5102/graphql`
    - BFF (Fusion): `http://localhost:5100/graphql`