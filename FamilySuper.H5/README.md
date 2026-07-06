# FamilySuper.H5

家庭超级管家智能体 H5 移动端 (Vue 3 + Vant)。

## 状态

本目录为占位,第一阶段未实现。H5 移动端将在后续阶段开发。

## 计划技术栈

- Vue 3 + Vite
- Vant 4 (移动端 UI 组件库)
- 通过 RESTful API 与 FamilySuper.API 通信
- 部署方式: 独立部署或嵌入 Blazor wwwroot

## API 端点

后端 API 默认监听 `http://localhost:5000`,主要端点:

- `GET /health` - 健康检查
- `GET /api/family/members` - 家庭成员列表
- `POST /api/family/members` - 添加家庭成员
- `GET /api/family/members/{id}` - 成员详情
- `PUT /api/family/members/{id}` - 更新成员
- `DELETE /api/family/members/{id}` - 删除成员(软删除)

请求头可携带 `X-App-Mode: adult|child` 标识当前模式。

## 计划功能

- 移动端对话界面
- 家庭成员管理
- 财务记录查看
- 健康档案查看
- 远程访问 (通过 frp 内网穿透)
