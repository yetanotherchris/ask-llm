class AskLlm < Formula
  desc "Ask any large language model from your terminal via OpenAI-compatible APIs"
  homepage "https://github.com/yetanotherchris/ask-llm"
  version "1.6.1"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.6.1/askllm-v1.6.1-osx-arm64"
      sha256 "12b8fd4800737f56063aeb2bccd71b58241af78ecdb8be877238b6d90225f416"
    else
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.6.1/askllm-v1.6.1-osx-x64"
      sha256 "35e527957ac7cc026f365b168fbe5e2aaf733a9107ec867b274aff9df2d2b18e"
    end
  end

  on_linux do
    url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.6.1/askllm-v1.6.1-linux-x64"
    sha256 "df6ad52e2213a19056d7576bcf38494029bccf1f1fbe9db1156d4b30cf0e4f89"
  end

  def install
    if OS.mac?
      if Hardware::CPU.arm?
        bin.install "askllm-v1.6.1-osx-arm64" => "askllm"
      else
        bin.install "askllm-v1.6.1-osx-x64" => "askllm"
      end
    else
      bin.install "askllm-v1.6.1-linux-x64" => "askllm"
    end
  end

  test do
    assert_match "USAGE:", shell_output("#{bin}/askllm --help")
  end
end






