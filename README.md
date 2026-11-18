# Human Outer Space Development Project

## 版本说明
Unity 6.3 LTS（6000.3.28f1）

## 大文件管理

本仓库使用 Git LFS 管理图片、模型、音视频、字体和 Excel 等二进制源文件；具体规则见 `.gitattributes`。Unity 场景、Prefab、文本资源及其 `.meta` 文件仍由普通 Git 跟踪。

协作者首次使用前，需要在自己的电脑上安装 [Git LFS](https://git-lfs.com/)，并执行：

```sh
git lfs install
git clone https://github.com/Xiuyuan2001/HumanOuterSpaceDevelopmentProject.git
```

如果先克隆后安装，可在仓库目录执行 `git lfs install` 和 `git lfs pull`。提交前使用 `git lfs ls-files` 检查大文件是否被 LFS 跟踪。新增大型二进制类型时，应先更新 `.gitattributes`，再添加文件；已有历史文件需要另行迁移，修改规则不会自动转换历史。

Unity 的 `Library`、`Temp`、`Build` 等生成目录不应提交。LFS 需要占用远端存储与下载额度，团队使用前请检查 Git 托管服务的配额和计费规则。
