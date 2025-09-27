class AskLlm < Formula
  desc "Ask any large language model from your terminal via OpenAI-compatible APIs"
  homepage "https://github.com/yetanotherchris/ask-llm"
  version "1.3.2"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.3.2/askllm-v1.3.2-osx-arm64"
      sha256 "61cf3349183efc2a27d828b9afed6f70e6771b2c350bed000a4a22cd67375deb"
    else
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.3.2/askllm-v1.3.2-osx-x64"
      sha256 "c6f884cb70997c9aa2d2f8fe4ea6e257892999efddbf73e702fb071645478be5"
    end
  end

  on_linux do
    url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.3.2/askllm-v1.3.2-linux-x64"
    sha256 "16308b44d4402f4e07bea3de92f715ebe1e29dba1bd1b2dae222a5c5516b77ef"
  end

  def install
    if OS.mac?
      if Hardware::CPU.arm?
        bin.install "askllm-v1.3.2-osx-arm64" => "askllm"
      else
        bin.install "askllm-v1.3.2-osx-x64" => "askllm"
      end
    else
      bin.install "askllm-v1.3.2-linux-x64" => "askllm"
    end
  end

  test do
    assert_match "USAGE:", shell_output("#{bin}/askllm --help")
  end
end



